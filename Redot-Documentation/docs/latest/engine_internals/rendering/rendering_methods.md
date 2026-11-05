# Rendering methods and drivers

A rendering method chooses how a scene is drawn. A driver supplies graphics API
operations. Keeping those choices separate explains which code is shared and why adding
one driver does not automatically add another scene pipeline.

| Method | Scene implementation | Graphics path | Shared canvas family |
|---|---|---|---|
| Forward+ (`forward_plus`) | [RenderForwardClustered](doc_rendering_classes_methods) | RD over Vulkan, D3D12, or Metal | [RendererCanvasRenderRD](doc_rendering_classes_rd) |
| Mobile (`mobile`) | [RenderForwardMobile](doc_rendering_classes_methods) | RD over Vulkan, D3D12, or Metal | `RendererCanvasRenderRD` |
| Compatibility (`gl_compatibility`) | [RasterizerSceneGLES3](doc_rendering_classes_gles) | OpenGL/OpenGL ES/WebGL; optionally a platform translation layer | [RasterizerCanvasGLES3](doc_rendering_classes_gles) |
| Dummy | [RasterizerSceneDummy](doc_rendering_classes_dummy) | No GPU scene drawing | [RasterizerCanvasDummy](doc_rendering_classes_dummy) |

Availability depends on the target, compiled drivers, settings, and capabilities.
Forward+ can fall back to the Mobile scene implementation during RD compositor
construction if the per-stage texture limit is below 48.

## Shared versus separate code

Forward+ and Mobile share server dispatch, scene culling, storage interfaces, RD
storage, the RD scene base, shader infrastructure, device validation, and native
drivers. They have different scene shader types, render-buffer custom data, render-list
decisions, and pass orchestration.

Compatibility shares the server/culling contracts but supplies separate GL storage,
canvas, scene rendering, shaders, and output handling. It does not translate RD draw
lists into OpenGL commands.

Dummy preserves enough server and resource behavior for headless engine operation. Its
no-op drawing methods are intentional. Testing with dummy can exercise scene/server code
without validating a GPU renderer.

## Pass policy

[Forward+](doc_rendering_forward_plus) combines clustered light/decal/probe lookup with
a larger set of optional depth, GI, screen-space, temporal, and post-processing work.

[Mobile](doc_rendering_mobile) uses a forward scene path organized around
attachment/subpass reuse where its active features permit it. Screen/depth reads,
scaling, effects, and callbacks can force pass splits.

[Compatibility](doc_rendering_compatibility) uses GL state and draw calls, with its own
resource limits, lighting passes, and post-processing rules. An API method accepted by
the shared server can be unsupported or ignored by one implementation; the shared method
signature is not a feature guarantee.

## Selection and translation

Native Metal is a device driver beneath RD. MoltenVK instead supplies a Vulkan
implementation on Metal beneath Redot's Vulkan path. Those are separate code routes.
Likewise, ANGLE supplies a GL-family route through a platform's ANGLE context manager;
it does not make Compatibility use Redot's D3D12 driver.

The [platform map](doc_rendering_platforms) identifies the selection points. Use the
[backend source map](doc_rendering_backend_sources) to follow a particular compiled
target rather than assuming every combination exists on every platform.

Sources: [compositor selection](doc_rendering_source_map#startup-and-dispatch), [scene
implementation headers](doc_rendering_classes_methods), and [native driver
headers](doc_rendering_classes_drivers).
