# Lighting and environments

Lighting crosses scene resources, spatial relationships, storage, shadow/GI preparation,
scene shaders, and GPU passes. There is no single lighting class that owns every part of
that path.

## Responsibilities

| Component | Role |
|---|---|
| Scene light/probe/environment resources | User-facing properties and server identities |
| [RendererSceneCull](doc_rendering_classes_core) | Light/geometry relationships, visibility, shadow work selection, probe update scheduling |
| Light storage | Light/probe/lightmap records, instance data, shadow and reflection atlases, GPU light buffers |
| [RendererEnvironmentStorage](doc_rendering_classes_storage) | Environment configuration consumed by renderers |
| [RendererCameraAttributes](doc_rendering_classes_storage) | Exposure and depth-of-field inputs |
| `RendererRD::SkyRD` | Sky shader/material state, radiance preparation, sky drawing |
| `RendererRD::GI` | VoxelGI/SDFGI data and GPU operations; tied to RD scene data |
| `RendererRD::Fog` | Fog volume state and associated rendering resources |
| Concrete scene renderer | Select and execute the lighting/GI/sky/fog passes supported by the method |

## Direct lighting and shadows

The scene culler finds lights and geometry that can influence the camera and prepares
shadow work. Directional shadow coverage differs from positional light coverage. Light
storage owns atlas allocation and light-instance information; concrete renderers record
the shadow and color passes.

Forward+ places light, probe, and decal data into its cluster representation. Mobile
prepares bounded/pairing-oriented light/probe/decal data for its scene shaders.
Compatibility has its own uniforms and additive lighting passes. Changing a shared light
property can require updates to all these paths.

Shadow caching depends on dirty state, transforms, visibility, camera coverage, and
resource changes. Avoid treating an atlas entry as valid based only on whether its
native texture still exists.

## Indirect lighting

Lightmaps are baked inputs sampled during scene rendering. The GPU lightmapper is a
module that creates a local RD and performs its own raster/compute and submit/sync work;
it does not run as another frame pass in the main viewport.

VoxelGI and SDFGI combine scene-culling preparation with GI-specific GPU work.
`RendererRD::GI` stores resource and instance data and works with [RenderDataRD](doc_rendering_classes_rd_storage), the
scene renderer, and buffer custom data. Its header explicitly notes that
[VoxelGIInstance](doc_rendering_classes_effects) is still used directly by render code pending a refactor. That is
evidence of an intended separation that remains incomplete.

Dynamic GI support is method-dependent. Mobile's capability override disables the
dynamic GI path; GLES3 supplies a different reduced implementation.

## Sky, fog, and probes

Sky radiance/reflection work and reflection probes can render additional views outside
the main camera's draw. Their render-buffer availability and effect eligibility differ
from normal viewport rendering.

Volumetric fog needs dedicated buffers, lighting/GI input, and method-specific
execution. Environment controls do not make its Forward+ path available to every
renderer. Camera feeds also have a Compatibility environment route, which introduces a
dependency on camera server data.

## Ownership detail

The RD compositor's GI interface comes from the scene renderer rather than a separately
allocated compositor-owned GI pointer. Fog is a separately constructed compositor
component. Preserve those different lifetimes when extracting environment services.

Sources: [scene preparation](doc_rendering_source_map#viewports-and-scenes),
[environment class index](doc_rendering_classes_effects), [light storage
index](doc_rendering_classes_rd_storage), and [lightmapper/camera
integration](doc_rendering_integration_sources#services-and-modules).
