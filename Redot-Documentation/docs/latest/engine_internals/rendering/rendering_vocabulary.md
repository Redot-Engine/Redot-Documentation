# Rendering vocabulary

Several renderer names describe different layers. This glossary separates those meanings
before following execution paths.

| Term | Meaning in this implementation |
|---|---|
| Rendering method | Scene rendering policy: Forward+, Mobile, Compatibility, or dummy |
| Rendering driver | Graphics implementation selected by name, such as `vulkan`, `d3d12`, `metal`, or `opengl3` |
| Display server | Platform window/display integration; creates graphics contexts and surfaces |
| Compositor | Internal [RendererCompositor](doc_rendering_classes_core) implementation that supplies renderer components and frame hooks |
| Compositor effect | Scene resource that registers callbacks at supported scene-render stages; distinct from `RendererCompositor` |
| Canvas | Retained 2D item hierarchy and drawing commands |
| Scenario | Server-side 3D world containing instances and spatial indexes |
| Instance | Placement and visibility state for a resource such as a mesh, light, or particle system |
| Storage | Component that owns renderer resource records and their GPU representations |
| Render target | Output resource managed by texture storage, used by a viewport or external output |
| Render scene buffers | Per-output intermediate images and method-specific data for 3D rendering |
| RID | Opaque identifier looked up by an owning subsystem; it does not imply object reference counting |
| Native handle | Graphics API object identity exposed for interoperation; its lifetime follows the API and owning component |
| Draw list | RD recording scope for graphics commands |
| Compute list | RD recording scope for compute dispatches |
| Command graph | RD's recorded command/resource dependency structure |
| Uniform set | Collection of shader bindings for one set index; maps to backend descriptors or arguments |
| Pipeline | Shader stages plus compatible fixed graphics state, or a compute program configuration |
| Pass | Context-dependent: a semantic scene stage, an RD draw list, or a native render-pass/subpass concept |
| Barrier | Ordering and visibility operation between resource uses; its implementation depends on the backend |
| Resolve | Conversion of multisampled data to a single sample per pixel |
| Frames in flight | Submitted frames whose GPU work can still reference resources |
| Dirty state | Cached state that must be rebuilt after a resource or relationship changes |

## Aliases and historical names

`RS` abbreviates [RenderingServer](class_RenderingServer); `RD` abbreviates [RenderingDevice](class_RenderingDevice). `RDD` and `RDG`
abbreviate the driver and device graph. `RSG` abbreviates [RenderingServerGlobals](doc_rendering_classes_core).
These names refer to different contracts even when they all appear in one function.

[RasterizerGLES3](doc_rendering_classes_gles) and [RasterizerDummy](doc_rendering_classes_dummy) implement `RendererCompositor`. Forward+ appears
in source as [RenderForwardClustered](doc_rendering_classes_methods); Mobile appears as [RenderForwardMobile](doc_rendering_classes_methods). The
`renderer_rd` directory contains the renderers above RD, not the Vulkan driver.

The word *wrapper* needs a qualifier. A resource wrapper can preserve scene ownership, a
dispatch wrapper can move work to another thread, a binding wrapper can adapt parameter
types, and a native wrapper can preserve GPU state and lifetime. Their names alone do
not tell whether a call can bypass them.

Continue with [architecture](doc_rendering_architecture) or the concrete [wrapper
chains](doc_rendering_wrappers).

Source: [core classes](doc_rendering_classes_core), [native driver
classes](doc_rendering_classes_drivers), and [resource/device
contracts](doc_rendering_source_map#resources-and-device).
