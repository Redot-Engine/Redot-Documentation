# RenderingDevice contracts

[RenderingDevice](class_RenderingDevice) provides the graphics-resource and command API
used by Forward+, Mobile, canvas rendering, and GPU tools. It contains substantial
validation, bookkeeping, synchronization, and submission logic above the native drivers.

## Three distinct interfaces

| Interface | Responsibilities |
|---|---|
| [RenderingContextDriver](doc_rendering_classes_core) | Enumerate devices, track window/surface relationships, query presentation support, construct device drivers |
| `RenderingDevice` | Own RD resources, validate formats/bindings, manage dependencies and frames, record commands and transfers |
| [RenderingDeviceDriver](doc_rendering_classes_core) | Create native resources, record native commands, implement queues, fences, swapchains, capabilities, and shader-container formats |

[RenderingDeviceCommons](doc_rendering_classes_core) supplies shared enums, descriptions, and reflection types. The
driver inherits these common definitions; it does not inherit the whole public
`RenderingDevice` implementation.

The context contract itself refers to `DisplayServer::WindowID` and VSync types. Surface
platform data enters through a `void *` contract whose layout the platform/context
implementation must agree on. The abstraction therefore has explicit platform
integration requirements.

## Resources and bindings

RD owns texture, buffer, shader, uniform-set, framebuffer, and pipeline records behind
separate RID owners. A native driver uses typed IDs such as `TextureID` and
`CommandBufferID`. RD records metadata needed to validate operations before passing
native work to the driver.

For example, `uniform_set_create()` checks reflected shader bindings, supplied uniform
types/counts, and referenced resources. It builds driver bindings and resource-usage
tracking. That function is more than a parameter rename around a native descriptor
allocation.

Public object wrappers such as [RDTextureFormat](doc_rendering_classes_core), [RDUniform](doc_rendering_classes_core), and
[RDPipelineRasterizationState](doc_rendering_classes_core) adapt the exposed API into C++ descriptions. Internal
renderer code can use the corresponding native C++ structs directly. These parameter
wrappers are distinct from scene resource ownership wrappers.

## Record commands

A draw list establishes framebuffer, attachment, viewport, pipeline, and binding state
for graphics commands. A compute list establishes state for dispatches. RD validates
their use and records commands in [RenderingDeviceGraph](doc_rendering_classes_core).

Framebuffer formats and pipeline formats must match. Texture usage flags must permit the
intended sampling, attachment, storage, or copy operation. A driver being able to create
a native resource does not bypass those RD contracts.

## Submit work

The main device's frame loop uses `swap_buffers()`. RD finishes lists and transfers,
asks the graph to record ordered commands, executes queues, and optionally presents
swapchains. The next frame slot waits before reuse and retires delayed deletions.

Local devices instead use `submit()` / `sync()`. GPU lightmapping and texture
compression use that facility independently of scene draw scheduling. Each device owns
its resources and submission state.

## Native escape routes

RD exposes driver resources and driver callbacks for integrations that need native APIs.
MetalFX uses this route: it obtains Metal texture/device objects, records a callback in
the graph with resource-use declarations, and encodes native MetalFX work when the
callback runs.

These routes make the abstraction boundary permeable. New native work must declare its
resources and respect command-buffer/lifetime rules; taking a raw handle alone does not
register a hazard with the command graph.

Sources: [device and context execution](doc_rendering_source_map#resources-and-device),
[driver classes](doc_rendering_classes_drivers), and [effect
classes](doc_rendering_classes_effects).
