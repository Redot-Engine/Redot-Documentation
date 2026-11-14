# Forward+ scene pipeline

[RenderForwardClustered](doc_rendering_classes_methods) implements Forward+ above [RendererSceneRenderRD](doc_rendering_classes_rd). It owns
method-specific surface lists, instance data, pipeline selection, and scene pass
orchestration. [SceneShaderForwardClustered](doc_rendering_classes_methods) handles its shader and material
representations.

## Inputs and retained data

The shared scene path supplies [RenderDataRD](doc_rendering_classes_rd_storage): camera/scene data, visible geometry,
light/probe lists, atlases, environment, compositor, and output buffers.
[RenderBufferDataForwardClustered](doc_rendering_classes_methods) adds method-specific attachments and a
[ClusterBuilderRD](doc_rendering_classes_rd) to the output's custom data.

[RenderGeometryInstance](doc_rendering_classes_core) implementations cache the surface and dependency state used to
build opaque, alpha, motion, and shadow work. A material or mesh change can invalidate
that state before a draw occurs.

## Cluster construction

`ClusterBuilderRD` receives light volumes, decals, and reflection probes. Its `begin()`
resets view-specific state; `bake_cluster()` produces buffers consumed by scene shaders.
Construction includes raster and compute work; describing it as only one compute
dispatch misses the implementation's intermediate representation and passes.

The cluster buffer lets a shader find relevant elements for a screen region and depth
range. It reduces how much light/probe/decal data each shaded fragment must consider. It
does not replace CPU scene visibility or shadow caster selection.

## Conditional pass sequence

<mermaid ref="./diagrams/rendering-forward-passes.mmd"
         alt="Forward+ conditionally prepares depth and GI, draws opaque geometry and sky, processes screen effects and transparency, then applies temporal and output processing." />

`_render_scene()` selects required attachments and passes from material, environment,
output, GI, antialiasing, and compositor flags. Its major stages include preparation,
depth/normal/roughness work when needed, light/cluster/GI preparation, opaque drawing,
sky/background, screen-space or separate-specular work, transparency, temporal
processing, and output processing.

These are conditional stages rather than a single fixed pass list. Reflection probes use
different buffers and skip work intended for the normal viewport. Debug modes and
material screen/depth requirements also change the path.

Compositor callbacks appear at pre-opaque, post-opaque, post-sky, pre-transparent, and
post-transparent positions. Requested color/depth resolves and extra attachments can add
work around those points. A callback that asks for normal/roughness, motion vectors,
albedo, or separate specular can affect earlier pass selection.

## Pipeline selection

Surface drawing combines shader version, pass mode, vertex format, framebuffer format,
blend/depth/raster state, and specialization choices. Compilation can be prepared in
background tasks and counted by source. The render loop must still handle whether the
required pipeline is available.

Scene shader code lives in the Forward+ shader family and shared lighting includes.
Generated `*ShaderRD` wrappers feed `ShaderRD`; RD then creates backend-specific shader
containers and pipelines. See [shaders](doc_rendering_shaders) and
[caches](doc_rendering_caches).

## Extension pressure

Adding a semantic pass currently means editing concrete orchestration, attachments,
resource bindings, and shader/pipeline choices. The device graph will track submitted
resource hazards, but it does not choose where that new scene stage belongs. This is a
central distinction for proposals to simplify the scene pipeline.

Sources: [Forward+ execution](doc_rendering_source_map#viewports-and-scenes), [method
classes](doc_rendering_classes_methods), [cluster builder](doc_rendering_classes_rd),
and [shader/effect entry points](doc_rendering_source_map#shaders-and-effects).
