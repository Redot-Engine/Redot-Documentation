# Storage and invalidation

Storage components translate retained renderer resources into data that draw code can
use. Their interfaces look parallel, but their responsibilities and dependencies are
broader than containers of GPU handles.

## Storage families

| Contract | Important responsibilities |
|---|---|
| [RendererTextureStorage](doc_rendering_classes_storage) | Texture allocation, upload, proxies, canvas textures, render targets, native/RD bridges |
| [RendererMaterialStorage](doc_rendering_classes_storage) | Shader and material records, parameters, global/instance uniforms, shader metadata |
| [RendererMeshStorage](doc_rendering_classes_storage) | Surface data, vertex/index representations, multimeshes, skeletons, mesh instances |
| [RendererLightStorage](doc_rendering_classes_storage) | Lights, shadow/reflection atlases, probes, lightmaps, instance data |
| [RendererParticlesStorage](doc_rendering_classes_storage) | Particle simulation state, process materials, draw meshes, collision inputs |
| [RendererUtilities](doc_rendering_classes_storage) | Resource recognition/free dispatch, dirty-resource updates, profiling, capability reporting |

The RD family uses namespace `RendererRD`; Compatibility uses `GLES3`; dummy uses
`RendererDummy`. Shared environment, camera-attribute, and compositor storage live
outside those backend families. Their implementations are not all supplied through the
same factory pattern.

The [shared](doc_rendering_classes_storage), [RD](doc_rendering_classes_rd_storage), and
[GLES3](doc_rendering_classes_gles) indexes enumerate the concrete headers.

## Resource change propagation

[Dependency](doc_rendering_classes_storage) stores registered trackers and sends changed/deleted notifications.
[DependencyTracker](doc_rendering_classes_storage) records the resources an instance used during an update. Its
`update_begin()` / `update_end()` sequence removes stale relationships after the current
dependencies have been enumerated.

`RendererSceneCull::_update_dirty_instance()` refreshes its base, materials, skeleton,
and particle-related dependencies. It determines properties such as shadow casting and
animation, refreshes instance uniforms, and updates the concrete geometry instance. A
material edit therefore affects visibility and render-list preparation as well as
parameter uploads.

Canvas items also track material changes. Their callbacks queue updates and detach
deleted material references. Those callbacks are part of the retained state model, not
optional editor notifications.

## Dirty resource ordering

Scene dirty-instance processing finishes by calling
`RSG::utilities->update_dirty_resources()`: instance changes can make resources dirty.
Backend utilities in turn update material, mesh, and related caches. Moving this work
independently can leave a geometry instance with stale surface state or shader
requirements for the current frame.

Material storage separates shared records from shader-type-specific `ShaderData` and
`MaterialData`. Scene shader implementations register creation callbacks. This supports
different material representations for canvas, particles, sky, Forward+, and Mobile, but
also couples storage to shader policy.

## Boundaries with exceptions

RD texture storage asks the concrete scene renderer for the HDR render-target format. RD
material storage also contains concrete Forward+/Mobile shader-data casts. These are
verified dependencies across the conceptual storage/rendering boundary; an abstract
storage base class does not eliminate them.

Storage cleanup can invalidate descriptor or framebuffer caches through RD dependencies.
A refactor must preserve both renderer notifications and GPU cache invalidation. See
[ownership](doc_rendering_ownership), [caches](doc_rendering_caches), and [boundary
observations](doc_rendering_boundaries).

Sources: [resource contracts](doc_rendering_source_map#resources-and-device),
[dirty-instance updates](doc_rendering_source_map#viewports-and-scenes), and [concrete
storage
dependencies](doc_rendering_observation_sources#wrapper-and-ownership-observations).
