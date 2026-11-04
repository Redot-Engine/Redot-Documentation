# Viewports and render buffers

[RendererViewport](doc_rendering_classes_core) coordinates outputs. It decides which viewports draw, orders them,
configures 3D intermediates, combines canvas and scene work, and hands final images to
screen or XR integration.

The scene [Viewport](class_Viewport) class owns scene-facing behavior. The server's viewport record is a
separate RID-owned object. It references a render target in texture storage, a shadow
atlas in light storage, and optional render scene buffers.

## Select and order outputs

`draw_viewports()` evaluates each active viewport's update mode, dimensions, screen
attachment, render-target use, and parent visibility. XR viewports override size/view
count from the primary XR interface and can require frame submission even when ordinary
viewport update rules would skip drawing.

The active viewport sort considers parent relationships and sibling texture-use
dependencies. Sibling ordering uses a topological sort; a detected cycle falls back to
the original sibling order and warns once.

Texture consumption is discovered around viewport drawing. The implementation snapshots
usage flags for active render targets, draws a viewport, then records newly observed
consumption in `used_by_viewport` for later sorting. That field stores one consumer
identity. This is a retained, observation-based ordering mechanism, not the RD command
graph and not a general declaration of every viewport read dependency.

## Combine 2D and 3D

`_draw_viewport()` checks whether a valid camera can draw 3D and whether 2D is enabled.
Its current canvas path requires a single view. An environment can request canvas layers
as a 3D background, changing where the 3D scene draw occurs relative to those layers.

The viewport manager prepares 2D lights, occluders, shadows, and optional canvas SDF
work, then asks the canvas culler to draw ordered items. It asks the scene culler to
render the camera/scenario for 3D. The concrete scene renderer produces the
method-specific intermediate image.

## Keep buffers distinct

| Object | Purpose |
|---|---|
| Render target | Output image and target state in texture storage |
| [RenderSceneBuffersConfiguration](doc_rendering_classes_storage) | Dimensions, scaling, samples, views, and other configuration passed into 3D buffers |
| [RenderSceneBuffers](class_RenderSceneBuffers) | Shared interface for a scene renderer's intermediate resources |
| [RenderSceneBuffersRD](class_RenderSceneBuffersRD) | RD texture collections, slices, scopes, and custom data |
| Method-specific custom data | Forward+/Mobile attachments and other method requirements |
| [RenderSceneBuffersGLES3](doc_rendering_classes_gles) | Compatibility framebuffer/texture intermediates |

Internal size and target size can differ because of 3D scaling. Multisampled, resolved,
history, velocity, and upscaled images may coexist. A new pass must declare which
resolution, view, layer, mip, and sample count it expects.

Changing size, view count, scaling, or sample configuration can rebuild buffers and
invalidate cached framebuffers/uniform sets. Avoid retaining borrowed intermediate
texture RIDs across reconfiguration without a validity strategy.

## Present images

RD collects [BlitToScreen](doc_rendering_classes_core) records by window and asks the compositor to prepare the
screen and draw output textures. The device later submits and presents. OpenGL
integrates current-window switching and buffer swaps along the viewport path. XR can
override color/depth/velocity targets, then return mirror blits.

Sources: [viewport execution and
sorting](doc_rendering_source_map#viewports-and-scenes), [buffer
classes](doc_rendering_classes_rd_storage), and [XR
integration](doc_rendering_integration_sources#services-and-modules).
