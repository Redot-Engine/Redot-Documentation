# Scene visibility and geometry

The 3D path separates a retained world and its visibility decisions from the concrete
renderer's pass execution. That separation still exchanges internal geometry objects,
storage metadata, and shadow/GI preparation data.

## Follow a mesh instance

[VisualInstance3D](class_VisualInstance3D) creates a server instance and records its scene object ID.
`MeshInstance3D::set_mesh()` associates a mesh resource with the instance. Transform,
visibility, material overrides, skeleton state, and world changes update separate server
properties. The mesh resource and its placement have different lifetimes.

[RendererSceneCull](doc_rendering_classes_core) retains instances and scenarios. A scenario holds spatial indexes
and lists for geometry, lights, probes, and related volumes. Instance dirty updates
rebuild bounds, dependency relationships, and the concrete [RenderGeometryInstance](doc_rendering_classes_core)
representation used by scene renderers.

<mermaid ref="./diagrams/rendering-scene.mmd"
         alt="Scene nodes update server instances and storage; the scene culler selects visible geometry and passes it to one of three scene renderer implementations." />

## Prepare visible work

`RendererViewport::_draw_3d()` asks the rendering method to render a camera.
`RendererSceneCull::render_camera()` constructs camera data and calls `_render_scene()`
with the chosen scenario and output buffers.

Preparation includes visibility-range dependencies, frustum tests, spatial index
queries, occlusion information, and light/probe relationships. Work can be split among
CPU worker tasks above configured thresholds. The result contains concrete geometry
instances and separate lists of lights, probes, decals, lightmaps, fog volumes, and GI
inputs.

Occlusion is another interface boundary: [RendererSceneOcclusionCull](doc_rendering_classes_core) has a module
implementation, `RaycastOcclusionCull`, backed by Embree. It is not a native
Vulkan/D3D12/Metal occlusion-query abstraction.

## Shadows and indirect lighting

The scene culler selects light shadow work and manages updates that depend on camera
coverage and instance relationships. Directional and positional lights have different
preparation. Shadow atlases and light instance records live in light storage; the
concrete scene renderer performs the GPU work.

SDFGI region updates, reflection probes, and particle colliders also have scene-culling
entry points. The culler consequently handles more than a visible/not-visible decision
for the main camera.

## Hand off to drawing

The culler calls `RendererSceneRender::render_scene()` with scene buffers,
current/previous camera data, visible resource lists, environment/compositor IDs,
atlases, and prepared shadow/GI work.

The RD base builds [RenderDataRD](doc_rendering_classes_rd_storage) and [RenderSceneDataRD](doc_rendering_classes_rd_storage), performs common preparation,
and delegates method-specific work to `_render_scene()` in Forward+ or Mobile. GLES3
implements the common scene-render interface through its own [RenderDataGLES3](doc_rendering_classes_gles) and GL
execution path.

Concrete renderers build and sort surface render lists, choose shader and pipeline
variants, upload instance data, and issue passes. The cull result is not a
ready-to-submit GPU command buffer.

## Change implications

A new geometry feature can affect bounds, dependency invalidation, CPU visibility,
geometry-instance data, surface lists, shadow passes, and shaders. Following only the
final draw function misses much of that contract. See [change
routes](doc_rendering_change_routes) and [lighting](doc_rendering_lighting).

Sources: [scene instance ownership](doc_rendering_integration_sources#scene-resources),
[scene execution](doc_rendering_source_map#viewports-and-scenes), and [occlusion
module](doc_rendering_integration_sources#services-and-modules).
