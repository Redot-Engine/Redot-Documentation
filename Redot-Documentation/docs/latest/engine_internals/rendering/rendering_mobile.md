# Mobile scene pipeline

[RenderForwardMobile](doc_rendering_classes_methods) shares RD infrastructure with Forward+ but uses different scene
shaders, geometry data, buffer formats, and pass decisions. Its main optimization is
preserving attachment work within compatible render-pass and subpass arrangements.

## Buffer and lighting choices

The method's internal scene color format is `DATA_FORMAT_A2B10G10R10_UNORM_PACK32`. Its
luminance multiplier accounts for the color scaling used with that format.
`_render_buffers_can_be_storage()` returns false for these scene buffers, selecting
raster alternatives in shared effects code where available.

This does not mean the engine disables every compute operation when Mobile is selected.
Particle simulation and other RD workloads have their own paths. The restriction belongs
to this scene-buffer/effect contract.

Mobile updates light, reflection-probe, and decal buffers and pairs relevant data with
geometry. It does not use Forward+'s [ClusterBuilderRD](doc_rendering_classes_rd) path. [SceneShaderForwardMobile](doc_rendering_classes_methods)
owns method-specific shader and material data.

## Subpass decisions

`_render_scene()` starts with the possibility of merging transparency and performing
output processing within subpasses. It then narrows that choice according to the active
features.

| Condition | Consequence in this path |
|---|---|
| Screen/depth texture reads | Can prevent keeping later drawing in the same subpass arrangement |
| Scaling or required post effects | Can move output processing to a separate pass |
| Screen-space antialiasing | Can prevent the final subpass output route |
| Pre-transparent compositor effect | Separates transparent work and disables subpass post-processing |
| Post-transparent compositor effect | Disables subpass post-processing |
| Reflection probe | Uses a different output path and does not use the normal final blit subpass |

The resulting execution can draw opaque, sky, transparency, and output work in one
compatible pass arrangement or split parts into separate lists/passes. This policy is
encoded in conditionals and framebuffer/pass construction, not in a declarative list of
interchangeable stage objects.

## Callback limits

This snapshot warns that post-opaque compositor effects are unsupported in the Mobile
renderer. It also warns that resolved depth for rendering-effect callbacks with MSAA is
unsupported. These restrictions remain even though the shared resource API can express
those callback types and flags.

When implementing another callback stage, account for attachment accessibility,
resolves, render-pass transitions, and tile-memory behavior. Inserting a function call
at the matching point is not enough.

## Feature boundaries

Dynamic GI support is disabled through the method's capability override. Forward+'s
screen-space and volumetric feature paths do not become available by choosing a
different native driver beneath Mobile. Shared code can choose raster post-processing
routes for supported effects, including depth of field, and the final result still
requires the method's color/luminance conventions.

Changes shared with Forward+ should be reviewed against both methods' buffer formats and
assumptions. In particular, a storage-image write to the main scene color buffer cannot
be introduced as a universal RD renderer operation.

Sources: [Mobile execution](doc_rendering_source_map#viewports-and-scenes), [method
headers](doc_rendering_classes_methods), and [callback
limitations](doc_rendering_observation_sources#incomplete-and-disabled-paths).
