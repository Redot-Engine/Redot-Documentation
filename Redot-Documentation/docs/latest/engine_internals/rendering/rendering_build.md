# Build and generated source map

The repository contains source for multiple renderers and platforms. A source index
describes available implementations, while build flags determine which ones exist in a
binary.

## Build entry points

`SConstruct`, `servers/SCsub`, driver/module configuration, platform detection, and
subordinate `SCsub` files establish renderer availability. The `servers/rendering/SCsub`
path adds shared rendering source, dummy, RD renderer, storage, and SPIR-V reflection
integration.

RD renderer subdirectories add effects, environment, Forward+, Mobile, shaders, and RD
storage. GLES3 has separate source/shader/storage/effect and environment build paths.
Vulkan, D3D12, and Metal have their own native driver and dependency setup.

Relevant guards include `RD_ENABLED`, `GLES3_ENABLED`, `VULKAN_ENABLED`,
`D3D12_ENABLED`, `METAL_ENABLED`, platform flags, `XR_DISABLED`, and generated module
enablement. Guard spelling and configuration belong to the selected revision; inspect
the containing file before assuming a build combination.

## Generated shader classes

`glsl_builders.py` reads stage sections and GLSL includes and generates C++ wrapper
headers such as `*.glsl.gen.h`. RD wrappers inherit `ShaderRD` and GLES3 wrappers
inherit [ShaderGLES3](doc_rendering_classes_gles). Shader `SCsub` files register these build steps. Edit the
template GLSL or builder, not a generated header in a local build directory.

The committed header inventory therefore does not enumerate every generated class that a
completed build contains. It indexes the template/build entry points and the committed
base classes instead.

## Compiler and native dependencies

The glslang module provides GLSL-to-SPIR-V compilation. SPIR-V reflection feeds RD
metadata; Vulkan loading/allocation, D3D12 allocation/NIR conversion, and Metal
SPIRV-Cross/framework integration supply native-specific operations. Their third-party
implementations are outside this manual's class inventory.

Compiled bytecode consumption and source compilation are different paths. Removing a
compiler module must be evaluated against runtime compilation, editor imports, baking,
and exported shader data rather than directory dependencies alone.

## Compatibility includes

`rendering_server.compat.inc`, `rendering_device.compat.inc`, and the RD scene buffer
compatibility include retain deprecated API adaptation alongside the current
implementations. These wrappers are distinct from the GL Compatibility rendering method.
Removal requires the engine's deprecation/compatibility policy and caller review; the
similar name does not imply GL-only use.

## Interpreting the inventory

The refresh tool reads committed Git objects at one revision. It excludes ignored build
outputs and uncommitted edits. Its class declarations are a lexical index, including
nested helper structs/classes, rather than a preprocessed C++ model. Header presence
does not assert feature availability or runtime construction.

The [source baseline](doc_rendering_baseline) records the revision, and the [reference
indexes](doc_rendering_reference) link each committed primary header to its declarations
and implementation. Use platform constructors and build guards to determine the
executable path for a specific target.

Sources: [shader generation](doc_rendering_source_map#shaders-and-effects), [native
headers](doc_rendering_classes_drivers), and [platform
selection](doc_rendering_backend_sources#opengl-and-platforms).
