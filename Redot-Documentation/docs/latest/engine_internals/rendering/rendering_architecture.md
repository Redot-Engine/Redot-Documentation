# Rendering architecture

The renderer has two execution families. Forward+ and Mobile use
[RenderingDevice](class_RenderingDevice), Redot's graphics abstraction over Vulkan,
Direct3D 12, and Metal. Compatibility calls OpenGL through the GLES3 implementation.
Dummy implements renderer contracts without GPU drawing.

The scene-facing [RenderingServer](class_RenderingServer) sits above both families.
Scene nodes and resources create server objects and submit changes through that API.
They do not submit a complete frame every time a property changes. Server components
retain state until frame execution consumes it.

<mermaid ref="./diagrams/rendering-layers.mmd"
         alt="Scene objects reach shared rendering services, which dispatch to either the RD renderer and a native driver or the GLES3 renderer." />

## Responsibilities

| Component | Responsibility | State it handles |
|---|---|---|
| [RenderingServerDefault](doc_rendering_classes_core) | Dispatch API calls, marshal threads, coordinate frames | Command queue and frame callbacks |
| [RendererCanvasCull](doc_rendering_classes_core) | Retain canvas items and select ordered 2D work | Canvas hierarchy and drawing commands |
| [RendererSceneCull](doc_rendering_classes_core) | Retain scenarios and instances, update dependencies, select visible work | Spatial indexes, cameras, instance relationships |
| [RendererViewport](doc_rendering_classes_core) | Schedule outputs and combine canvas, scene, and XR work | Viewports, render targets, render-buffer references |
| [RendererCompositor](doc_rendering_classes_core) implementations | Supply concrete renderer/storage components and frame hooks | Renderer implementation composition |
| [RendererSceneRender](doc_rendering_classes_core) implementations | Choose passes and draw visible 3D geometry | Render lists, scene shader state, method-specific buffers |
| Storage implementations | Maintain renderer resources | Meshes, materials, lights, textures, particles |
| `RenderingDevice` | Validate and track graphics resources and record commands | Device RIDs, resource dependencies, frames in flight |
| Context and device drivers | Integrate native graphics APIs | Devices, surfaces, native resources, queues, swapchains |

The [core class index](doc_rendering_classes_core) and [storage class
index](doc_rendering_classes_storage) link these contracts to their headers and
implementations.

## Composition and access

`RenderingServerDefault::_init()` creates the canvas culler, viewport manager, scene
culler, and camera attributes. It creates the selected compositor through
`RendererCompositor::create()`, then obtains its storage and drawing interfaces. The
scene culler receives the concrete scene renderer through `set_scene_render()`.

[RenderingServerGlobals](doc_rendering_classes_core), abbreviated `RSG`, publishes pointers to those components.
This is a shared access mechanism; it does not express ownership. `RSG::scene` points to
a [RenderingMethod](doc_rendering_classes_core), normally `RendererSceneCull`. `RSG::rasterizer` points to a
compositor. Historical names make these fields less descriptive than their types.

The RD compositor constructs RD storage objects and a shared canvas renderer, then
selects [RenderForwardClustered](doc_rendering_classes_methods) or [RenderForwardMobile](doc_rendering_classes_methods) for 3D. The GLES3 compositor
supplies a separate canvas renderer, scene renderer, and storage family. This
distinction matters: Compatibility does not implement another [RenderingDeviceDriver](doc_rendering_classes_core).

<mermaid ref="./diagrams/rendering-component-families.mmd"
         alt="RD, GLES3, and dummy compositors implement the compositor interface." />

<mermaid ref="./diagrams/rendering-scene-family.mmd"
         alt="Forward+ and Mobile specialize the RD scene renderer; GLES3 and dummy implement the shared scene interface separately." />

<mermaid ref="./diagrams/rendering-canvas-family.mmd"
         alt="RD, GLES3, and dummy canvas renderers implement the shared canvas interface." />

## Boundaries to preserve

The broad separation is scene semantics, retained server state, rendering policy, and
native GPU execution. The implementation crosses that separation through global
singletons, shared data structures, shader resources, and platform integration. Those
crossings are described in the [dependency map](doc_rendering_dependencies).

[RenderingDeviceGraph](doc_rendering_classes_core) is a command/resource dependency graph below the rendering
methods. It does not own the semantic list of Forward+ or Mobile passes. Changing the
device graph alone therefore does not provide a new scene pipeline API. See [wrapper
chains](doc_rendering_wrappers) and [boundary observations](doc_rendering_boundaries).

Source: [startup and dispatch](doc_rendering_source_map#startup-and-dispatch), [scene
execution](doc_rendering_source_map#viewports-and-scenes), and [backend
selection](doc_rendering_backend_sources#opengl-and-platforms).
