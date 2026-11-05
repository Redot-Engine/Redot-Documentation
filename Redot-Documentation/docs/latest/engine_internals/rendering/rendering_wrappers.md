# Wrapper chains and indirection

The renderer contains nested adapters, but their responsibilities differ. Evaluate what
state, thread, ownership, or representation each layer changes before deciding whether
to remove it.

## Texture creation chain

<mermaid ref="./diagrams/rendering-wrapper-chain.mmd"
         alt="Texture creation chooses direct or queued initialization, then the selected storage follows an RD, OpenGL, or dummy path." />

| Layer | Work performed | What a bypass must preserve |
|---|---|---|
| [ImageTexture](class_ImageTexture) / [Texture2D](class_Texture2D) | Resource identity, dimensions, draw behavior, change signals | Scene and editor resource semantics |
| [RenderingServer](class_RenderingServer) | Shared scene-facing API and RID domain | Backend-independent calls and exposed behavior |
| [RenderingServerDefault](doc_rendering_classes_core) macros | Choose storage destination and direct/queued execution | Thread affinity, ordering, allocation/initialization split |
| Concrete texture storage | Format conversion, texture metadata, proxies, color views, render-target integration | Resource relationships and renderer-visible identity |
| [RenderingDevice](class_RenderingDevice) | Validate graphics descriptions, own RD RIDs, track usage/dependencies and transfers | Hazard tracking, validity, deferred destruction |
| Native device driver | Create API resources and record native operations | Platform/API format, memory, synchronization, and lifetime |

Some calls can use a shorter route because their contract is narrower. Internal renderer
code can construct RD C++ parameter structs without the object wrappers used by the
exposed RD API. That does not make bypassing scene storage valid for every texture.

The texture creation wrappers initialize directly when the caller is on the server
thread **or** the compositor allows asynchronous resource creation. Otherwise they queue
initialization through `CommandQueueMT`. The RD compositor allows asynchronous creation;
GLES3 and dummy do not. Queueing and backend selection are separate decisions: GLES3
storage uses OpenGL directly, and dummy storage does not allocate a GPU texture.

## Draw execution chain

The common 3D path crosses [RendererViewport](doc_rendering_classes_core),
[RenderingMethod](doc_rendering_classes_core) / [RendererSceneCull](doc_rendering_classes_core),
and the [RendererSceneRender](doc_rendering_classes_core) interface. The culler chooses
visible inputs before handing them to the selected scene implementation.

Forward+ and Mobile continue through an RD scene base, a concrete scene method, RD,
the command graph, and a native driver. The concrete method chooses semantic passes;
RD validates and orders resource operations; the driver translates native commands.
Compatibility instead uses [RasterizerSceneGLES3](doc_rendering_classes_gles) and OpenGL
directly, without an RD command graph. The dummy scene implementation performs no GPU
draws.

Those jobs are not repeated merely because they use similar verbs such as render, draw,
update, or prepare. However, the large handoff argument list, shared mutable data, and
concrete-type accesses make changes hard to trace.

## Shader wrapper chain

Engine shader preprocessing/compiler output enters generated shader templates. Generated
classes adapt those templates to `ShaderRD` or [ShaderGLES3](doc_rendering_classes_gles). RD shader compilation adds
reflection and a backend container, then native pipeline creation adds fixed state and
format compatibility.

These are representation changes, not equivalent copies of one shader object. Cache
identity and invalidation also differ at each stage.

## Observations

Dispatch macros hide destinations in surrounding `ServerName` / `server_name`
definitions. Global aliases hide where collaborators came from. A developer following a
method name can consequently cross several files before seeing the operation that
changes state.

Parameter-wrapper conversion, retained state, thread dispatch, backend translation, and
lifetime tracking should be counted separately in an indirection audit. A virtual-call
count alone does not identify redundant work or a measured performance problem.

## Proposals

Start with traceability: explicit operation-to-owner maps, named parameter objects for
wide handoffs, and narrowly typed dependency accessors. Then evaluate removing duplicate
conversions or forwarding layers whose callers can satisfy the same contract directly.

Replacing the dispatch macros may improve navigation, but the replacement must preserve
queued/direct behavior, return synchronization, and asynchronous creation exceptions.
Replacing scene pass organization is a separate project from removing parameter
adapters.

Sources: [dispatch and construction](doc_rendering_source_map#startup-and-dispatch),
[device contracts](doc_rendering_source_map#resources-and-device), and [shader
transformations](doc_rendering_source_map#shaders-and-effects).
