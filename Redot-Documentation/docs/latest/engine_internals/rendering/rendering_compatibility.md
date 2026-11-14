# Compatibility and OpenGL

Compatibility is a complete renderer family above the shared server/culling contracts.
It uses GL-family APIs directly and bypasses [RenderingDevice](class_RenderingDevice), its command graph, and
its native-driver interface.

## Component family

[RasterizerGLES3](doc_rendering_classes_gles) implements compositor/frame hooks. It constructs
[RasterizerCanvasGLES3](doc_rendering_classes_gles), [RasterizerSceneGLES3](doc_rendering_classes_gles), `GLES3::Config`, and GLES3 storage
components. [ShaderGLES3](doc_rendering_classes_gles) manages GL shader versions and compilation.
[RenderSceneBuffersGLES3](doc_rendering_classes_gles) holds the method's intermediate framebuffer/texture state.

The code's GLES3 name covers desktop OpenGL, OpenGL ES, and the web route. The platform
establishes the context and selects whether GLES runs over desktop GL or through an ES
interface. Web uses WebGL 2 through Emscripten and browser integration.

## Scene drawing

The shared scene culler passes visible geometry, lights, probes, and other inputs to
`RasterizerSceneGLES3::render_scene()`. The implementation prepares environment state,
render lists, light/shadow data, framebuffer attachments, and GL draw state.

Surface drawing selects shader variants and binds vertex/index data, materials,
textures, and instance state. Light/shadow combinations can require additive lighting
passes. Depth, alpha, blending, and material requirements determine which passes can be
combined. These decisions differ from Forward+ clustering and Mobile's subpass policy.

GLES3 particle storage uses transform feedback for GPU particle updates. This is another
reason the Compatibility path cannot be modeled as an RD backend with a few missing
features.

## State and presentation

GL commands operate on a current context and mutable API state. The viewport manager
switches the current window where needed. `RasterizerGLES3::gl_end_frame()` asks
`DisplayServer` to swap buffers when presentation is requested; otherwise it calls
`glFinish()`.

`RasterizerGLES3::end_frame()` ends capture timestamps; it is distinct from
`gl_end_frame()`. Refactoring both into one generic frame hook requires preserving
multi-window and context behavior.

Texture, material, mesh, and lighting storage track GL object identities and
configuration. Their resource and cache rules are separate from RD's RID dependency
graph and delayed native deletion.

## Translation layers

Native GL/ES context managers use WGL, GLX/EGL, Apple context support, or
platform-specific glue as appropriate. Windows and macOS also have ANGLE context manager
implementations. ANGLE handles translation below the GL-family API. Redot's
[RenderingDeviceDriverD3D12](doc_rendering_classes_drivers) and native Metal driver are not involved in that
Compatibility translation route.

## Unsupported operations

The shared scene API contains feature controls that the GLES3 implementation does not
implement, including several Forward+ GI and screen-space effect quality operations.
Empty implementations can represent intentional method limits, rather than half-written
support. Trace capability checks and the relevant render path before classifying them as
defects.

The GL constructor also contains a source marker about failing during GLAD
initialization before all components exist. See [unfinished
paths](doc_rendering_unfinished) for that failure-lifetime concern.

Sources: [GL implementation and
contexts](doc_rendering_backend_sources#opengl-and-platforms), [GLES3 class
index](doc_rendering_classes_gles), and [scene
orchestration](doc_rendering_source_map#viewports-and-scenes).
