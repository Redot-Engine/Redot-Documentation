# Canvas rendering

The 2D path retains drawing commands in canvas items, culls and orders those items, and
dispatches them to an RD or GLES3 canvas renderer. It shares resource storage and
viewport outputs with 3D without using the 3D scene pass pipeline.

## Follow a textured rectangle

A [Sprite2D](class_Sprite2D) handles `NOTIFICATION_DRAW` by calculating source/destination rectangles
and calling its texture's `draw_rect_region()`. A custom
`CanvasItem::draw_texture_rect()` also delegates to a texture resource. Texture wrappers
can adjust regions or transform the draw before it reaches the server; an atlas texture
is a reason this resource-level delegation exists.

The server dispatches the canvas call to [RendererCanvasCull](doc_rendering_classes_core).
`canvas_item_add_texture_rect()` and related methods append command objects to the
retained item. Those commands contain geometry parameters, color, and server texture
identities. They are not immediate native GPU draw calls.

<mermaid ref="./diagrams/rendering-canvas.mmd"
         alt="A texture draw becomes a retained canvas command, then culling produces an ordered item list for an RD or GLES3 canvas renderer." />

## Cull and order items

[RendererViewport](doc_rendering_classes_core) gathers the viewport's canvases, lights, and occluders.
`RendererCanvasCull::render_canvas()` traverses item trees with transforms, visibility
masks, clipping, modulation, and ordering state. `_render_canvas_item_tree()` builds
ordered Z lists and hands the resulting linked item list to
`RendererCanvasRender::canvas_render_items()`.

Y sorting and physics interpolation affect this preparation. The implementation applies
interpolation and transform snapping before Y sorting so ordering matches the displayed
positions. Moving sorting into a later GPU stage would need equivalent visibility,
clipping, and ordering semantics.

Canvas commands and material dependencies persist between draws until an update changes
them. Changes to a material can invalidate retained item data even when the node did not
issue another explicit draw call.

## Execute canvas work

[RendererCanvasRenderRD](doc_rendering_classes_rd) interprets items and builds the GPU data, texture bindings,
batches, and draw lists needed for RD execution. [RasterizerCanvasGLES3](doc_rendering_classes_gles) performs
corresponding work through GL state and draw calls. Their buffers, shader variants, and
batching details differ.

Batch boundaries can arise from textures/materials, clipping, blending, command type,
lighting, and screen-texture requirements. Combining adjacent items without preserving
those state differences changes the image.

Canvas lights and occluders introduce additional preparation and shadow work. Canvas SDF
generation has its own render-target usage path, and shaders or particles can consume
the result. These resources belong to the canvas/output contracts rather than the 3D
light-cluster system.

## Shared infrastructure

Canvas rendering uses texture and material storage, viewport render targets, profiling,
and backend resource lifetimes. In the RD family it shares RD and caches with the
selected 3D renderer. Selecting Mobile changes the 3D scene implementation; it does not
select a second Mobile-only canvas renderer.

Mode7 adds a scene-level example: `Mode7Sprite2D` constructs a shader material and
scanline texture through existing resources. Its feature logic belongs to that consumer,
even though rendering API changes can affect it. Track this dependency before treating a
resource wrapper as unused indirection.

Sources: [scene draw entry points](doc_rendering_integration_sources#scene-resources),
[canvas culling and execution](doc_rendering_source_map#viewports-and-scenes), and
[canvas implementation headers](doc_rendering_classes_rd).
