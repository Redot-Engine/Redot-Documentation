# Device command graph

[RenderingDeviceGraph](doc_rendering_classes_core) records graphics operations and their resource uses, establishes
dependencies, groups barriers, and emits native commands. Its nodes are recorded
operations, not scene nodes or named scene-render stages.

## What enters the graph

RD records buffer and texture updates/copies/readbacks, draw lists, compute lists,
timestamps, and driver callbacks. Resources have trackers for their uses and required
access/layout state. Texture slices and shared resources need relationships to the
underlying allocation, not only independent RID comparisons.

<mermaid ref="./diagrams/rendering-command-graph.mmd"
         alt="Recorded uploads, draw lists, compute lists, and callbacks form resource dependencies that are ordered and translated into driver commands and barriers." />

Read/write relationships constrain ordering. Two operations that look independent in
source can depend on the same image or buffer through a uniform set, attachment, shared
view, or callback declaration.

## Finish recording

At the frame boundary, RD's `_end_frame()` submits transfer-worker work and barriers,
then calls `draw_graph.end()`. The graph waits for any secondary recording tasks and
emits commands into the frame's native command buffers.

With command reordering enabled, `end()` traverses adjacency lists, computes dependency
degrees and levels, and groups command types within allowed ordering. It groups
synchronization barriers between levels and invokes the driver operations needed to
record each command.

The graph receives a render-pass creation callback from RD. This lets it choose
attachment load/store behavior and native pass construction while RD retains its
resource and framebuffer-format contracts.

## Synchronization and workarounds

The source enables graph reordering and disables full barriers by default. The
full-barrier switch is intended for synchronization debugging and has a documented
performance cost. A driver/context workaround can also split command buffers, including
the `avoid_compute_after_draw` case. RD chains resulting buffers with synchronization at
submission.

The source includes machinery for recording secondary command buffers in worker tasks.
`SECONDARY_COMMAND_BUFFERS_PER_FRAME` is zero in this snapshot; the comment cites
unresolved behavior with some hardware vendors. Its presence in the graph does not
establish that normal frame recording is parallelized through that path.

## Boundaries of the graph

The graph does not decide which Forward+ depth, GI, transparency, or tone-map stage
should exist. Concrete scene renderers make those decisions before RD records their
operations. Viewport texture-consumer ordering is also a separate mechanism above RD.

Removing a scene wrapper does not remove a GPU hazard. Replacing the graph with direct
native calls would require preserving resource tracking, load/store choices, layouts,
workarounds, and frame submission dependencies.

## Investigation route

For a synchronization problem, start with the RD operation and identify the resource
tracker and usage it supplies. Follow recorded command dependencies, barrier groups, and
the driver's barrier implementation. For native callbacks, inspect the declared
resources as well as the native commands.

For a pipeline organization problem, start above the graph in the concrete scene method.
The [boundary proposals](doc_rendering_boundaries) describe a possible semantic stage
contract separately from this existing graph.

Sources: [graph and submission](doc_rendering_source_map#resources-and-device) and
[disabled secondary
recording](doc_rendering_observation_sources#wrapper-and-ownership-observations).
