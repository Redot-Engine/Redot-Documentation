# Materials and shader compilation

Shader processing has several stages with different inputs and owners. Scene shader
syntax, generated renderer templates, intermediate bytecode, backend containers, and
native pipelines are distinct representations.

## Scene material path

[ShaderMaterial](class_ShaderMaterial) and generated material resources submit shader code and parameters
through the server. Material storage owns the server shader/material records and creates
shader-type-specific `ShaderData` and `MaterialData` using callbacks registered by
canvas, scene, particle, and sky implementations.

[ShaderPreprocessor](doc_rendering_classes_core) handles preprocessing; [ShaderLanguage](doc_rendering_classes_core) parses and checks the
engine shader language; [ShaderCompiler](doc_rendering_classes_core) produces generated code and metadata for the
renderer's templates. Uniform layout and instance/global parameter packing connect the
compiler to material storage and [VariantConverterStd140](doc_rendering_classes_storage) helpers.

Changes to shader metadata can affect shadow casting, alpha classification, screen/depth
reads, animation, geometry-instance state, and required passes. Treating shader
compilation as only a GPU program replacement misses those scene-level consequences.

## Renderer templates

Renderer GLSL templates and includes live with RD or GLES3 shader families.
`glsl_builders.py` generates C++ shader wrapper headers from these files. Generated RD
classes inherit `ShaderRD`; GLES3 uses generated [ShaderGLES3](doc_rendering_classes_gles) classes. The generated
wrappers embed template source and configuration hooks; they are not separate graphics
drivers.

`ShaderRD` combines template code, custom code, variants, and specialization choices. It
can prepare and cache variant data, then create RD shaders from compiled backend
bytecode. [RenderingShaderLibrary](doc_rendering_classes_core) provides a renderer-side contract for shader access;
[ShaderIncludeDB](doc_rendering_classes_core) resolves engine include content.

## RD compilation path

<mermaid ref="./diagrams/rendering-shaders.mmd"
         alt="Engine shader code and templates produce GLSL, glslang produces SPIR-V, then native containers produce Vulkan bytecode, DXIL, or Metal code; GLES3 compiles a separate GL path." />

The GLSL compilation path uses the glslang module to produce SPIR-V.
`RenderingDevice::shader_compile_binary_from_spirv()` reflects the intermediate code,
asks the driver for its [RenderingShaderContainerFormat](doc_rendering_classes_core), creates a container, adds
reflection and native code, and serializes it.

| Backend | Container responsibility |
|---|---|
| Vulkan | Preserve/package SPIR-V, with compression/debug choices |
| D3D12 | Convert SPIR-V through NIR to DXIL where the compilation path is enabled; preserve resource/root-signature and specialization metadata |
| Metal | Translate through SPIRV-Cross and prepare Metal shader data and reflection |

The device then creates a shader resource from the container. Pipeline creation combines
it with compatible graphics or compute state. Source compilation availability depends on
module/build configuration; consuming previously compiled bytecode is a separate
operation.

## GLES3 path

Compatibility uses its own templates, generated wrappers, shader versions, uniform
handling, and GL program compilation/linking. It does not consume RD's native shader
containers. Shared engine shader-language changes still require checking both
compiler/template families.

## Coupling to resources

Both shader-language and preprocessor headers include the scene `ShaderInclude` resource
header. RD compilation conditionally includes the glslang module interface. Those
dependencies cross the apparent directory layers and are listed in the [boundary
observations](doc_rendering_boundaries).

Sources: [shader entry points](doc_rendering_source_map#shaders-and-effects), [compiler
module](doc_rendering_integration_sources#services-and-modules), and [backend
containers](doc_rendering_backend_sources).
