# Effects and compositor callbacks

Effects consume scene attachments and can require earlier rendering work. Their
integration depends on the rendering method, buffer formats, resolution, sample count,
temporal state, and callback location.

## Effect components

| Classes | Work they organize |
|---|---|
| `BokehDOF`, [Luminance](doc_rendering_classes_effects), [ToneMapper](doc_rendering_classes_effects) | Depth of field, exposure/luminance processing, output color conversion |
| [SSEffects](doc_rendering_classes_effects) | Screen-space ambient/indirect lighting, reflections, and related scene effects |
| `TAA`, [FSR2Effect](doc_rendering_classes_effects), [FSR2Context](doc_rendering_classes_effects) | Temporal processing with history, depth, motion, and jitter inputs |
| [SpatialUpscaler](doc_rendering_classes_effects), `FSR`, [MFXSpatialEffect](doc_rendering_classes_effects) | Spatial scaling through a shared effect interface and concrete implementations |
| [MFXTemporalEffect](doc_rendering_classes_effects) | Native MetalFX temporal work |
| `SMAA`, `Resolve` | Antialiasing and attachment resolve operations |
| `CopyEffects`, [SortEffects](doc_rendering_classes_effects), [DebugEffects](doc_rendering_classes_effects) | Shared GPU utility passes |
| [RoughnessLimiter](doc_rendering_classes_effects), `VRS`, [MotionVectorsStore](doc_rendering_classes_effects) | Shading-rate/roughness support and motion-vector processing |

These classes own shader variants, pipelines, bindings, and dispatch/draw parameters as
required. They do not all participate in every scene or method. The [effect class
index](doc_rendering_classes_effects) links their headers.

## Buffers and history

Shared RD scene code handles much of post-processing and tone mapping. It chooses
compute or raster paths according to whether the scene buffers support storage-image
use. It distinguishes internal color size from output target size and can select an
upscaled texture as the source.

Temporal processing needs current/previous camera and geometry data, motion vectors,
history images, exposure, and reset rules. Resizing or changing configuration can
invalidate history. A pass inserted between these operations must use compatible
color/depth conventions and preserve temporal inputs.

The source has markers about target/internal size choices, FSR2 buffer allocation, an
unimplemented transparency/composition mask, and temporal accumulation reset. These
deserve separate assessment from ordinary TODO cleanup.

## Compositor callback path

The scene [Compositor](class_Compositor) and [CompositorEffect](class_CompositorEffect) resources submit effect lists, callback
types, flags, and callables into shared compositor storage. The RD scene renderer looks
up the active effects for a stage and invokes callbacks with the current render data.

Forward+ calls the pre-opaque, post-opaque, post-sky, pre-transparent, and
post-transparent stages. Mobile supports a narrower arrangement and may break subpass
merging for callbacks. Compatibility does not gain these RD stages through the shared
resource type.

Flags such as resolved color/depth, motion vectors, normal/roughness, albedo, and
separate specular can require attachments or resolves before the callback. This couples
callback registration to pass planning. A callback's cost is not limited to the commands
inside its function.

## Callback lifetime and native work

Render data describes the current render invocation. Buffer RIDs and native resources
can change on reconfiguration. Retaining them without checking the owning buffers can
leave a later callback using invalid resources.

MetalFX uses a separate native driver-callback mechanism. It registers texture uses with
RD's graph, then encodes native work into the current Metal command buffer. This route
has a concrete driver dependency and completion-lifetime requirements, described in the
[Metal chapter](doc_rendering_metal).

Sources: [effect and callback execution](doc_rendering_source_map#shaders-and-effects),
[scene compositor resources](doc_rendering_integration_sources#scene-resources), and
[Mobile/temporal
limitations](doc_rendering_observation_sources#incomplete-and-disabled-paths).
