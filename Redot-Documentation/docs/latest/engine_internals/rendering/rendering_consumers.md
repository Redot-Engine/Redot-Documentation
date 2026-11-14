# Consumers outside the renderer

Renderer changes affect engine services and tools that do not draw the main scene. This
page identifies representative contracts to preserve; the [generated
indexes](doc_rendering_reference) enumerate source search leads.

## Core loading and pacing

`ResourceLoader::_ensure_load_progress()` calls renderer `sync()` when the engine uses
single-thread rendering. The loader can need queued renderer work to complete before a
resource load progresses. A source TODO proposes a more general server-progress
mechanism. Replacing the queue without preserving progress can affect threaded loading
even if scene drawing still works.

`Engine::set_max_fps()` obtains the main RD and forwards the limit. This makes
frame-limit configuration depend on a graphics-device implementation from core code. A
future pacing interface must preserve what RD does with that limit, including the
absence of RD on the Compatibility path.

## GPU tools and import

`LightmapperRD` creates a local device, creates its own resources, and submits and
synchronizes baking work. It also uses scene geometry inputs and module
raycasting/denoising infrastructure. Changing RD compilation, formats, or local-device
availability can break baking independently of realtime rendering.

`BetsyCompressor` first requests a local RD through the rendering server. If that fails
and the build supports the route, it can construct a Metal or Vulkan context and RD
directly. It has its own worker/queue lifecycle and compression shaders. The header
includes the private default-server header, although the implementation's server calls
use the public API.

Editor previews, image import, shader editors, and visual shader tools consume
resources, rendering metadata, offscreen viewports, or GPU operations. Editor operation
with a Compatibility scene renderer does not imply that every auxiliary GPU tool uses
only GL.

## XR and camera

XR interface calls occur around the server frame and viewport draw. The runtime supplies
view count, output size, color/depth/velocity textures, and frame timing. OpenXR
platform extensions also hook native Vulkan/D3D12/Metal setup and graphics bindings.
These are both high-level and native dependencies.

Camera feeds own server textures, update them from incoming images/native sources, and
defer appropriate scene-facing signals. GLES3 environment code also consumes camera feed
data. Texture ownership and external-thread updates must be preserved across camera
integrations.

## Geometry and output consumers

Physics soft bodies update renderer mesh data through a rendering-server handler.
Navigation, physics debug views, CSG, GridMap, and terrain modules produce or update
resources consumed by rendering. Their dependencies are not limited to editor-only
visualization.

Text servers create/update font atlas textures that canvas rendering samples. Movie
capture reads a viewport texture through the server, handles format/HDR requirements,
and collects render timing. A readback or render-target change can therefore affect text
or capture without touching scene draw code.

Mode7 constructs shader/material and scanline-texture resources. WorldScape creates and
updates terrain meshes, material/shader parameters, generated textures, and instances
through resource/server APIs. They are source-backed Redot consumers to include in
change review.

Sources: [services and modules](doc_rendering_integration_sources#services-and-modules),
[Redot scene/module resources](doc_rendering_integration_sources#scene-resources), and
the [dependency map](doc_rendering_dependencies).
