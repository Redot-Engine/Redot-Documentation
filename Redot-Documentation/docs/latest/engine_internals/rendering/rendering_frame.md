# Frame execution

A frame updates retained renderer state, produces commands for selected viewports, and
submits the resulting GPU work. Server synchronization, command recording, GPU
execution, and presentation occur at different points.

<mermaid ref="./diagrams/rendering-frame.mmd"
         alt="The main loop synchronizes the server, then a render frame updates scenes, particles, probes, and viewports before device submission." />

## Main loop decision

The main loop calls `RenderingServer::sync()` before scheduling rendering. It considers
whether the display can draw, whether there are additional outputs, whether the render
loop is enabled, and whether RD has pending resource processing. Low-processor-usage
mode also consults `has_changed()`. An engine iteration does not imply an unconditional
presented frame.

`RenderingServerDefault::draw()` requires the main thread. It emits `frame_pre_draw`,
resets its change counter, and either queues `_draw()` or calls it directly. A queued
draw can outlive the return from `draw()`.

## Server frame order

`RenderingServerDefault::_draw()` performs these operations in order:

1. Call the compositor's `begin_frame()` and start timestamps.
2. Call XR `pre_render()` when XR exists.
3. Update the scene culler, then the canvas culler.
4. Update particles after scene instances and collision inputs.
5. Render probes.
6. Call `RendererViewport::draw_viewports()`.
7. Update the canvas renderer, then call compositor `end_frame()`.
8. Call XR `end_frame()` and update visibility notifiers.
9. Arrange frame callbacks and collect profiling/memory information.

`RendererSceneCull::update()` also updates dirty instances/resources and renders
particle colliders. Moving particle simulation earlier can consume stale collider or
transform data.

## Viewport execution

[RendererViewport](doc_rendering_classes_core) evaluates update modes, sizes, output attachments, and XR
requirements. It sorts active outputs and records texture-use relationships. Each
selected viewport combines 3D and canvas work according to its environment/background
settings. The viewport's scene renderer chooses its own pass structure.

For RD outputs, screen blits are collected by window and issued through the compositor
when presentation is requested. For OpenGL, context switching and `gl_end_frame()`
happen at the viewport/window integration points. Do not move all GL buffer swaps to the
RD frame boundary.

## Submit and retire

The RD compositor's `end_frame()` calls `RenderingDevice::swap_buffers()`. RD closes
command recording, flushes the command graph, submits queues, optionally presents,
advances the frame slot, and begins recording again. Reusing a frame slot waits for the
relevant GPU completion and retires its deferred resource deletions.

Frame callbacks are CPU-side notifications. On the separate-render-thread path,
`_run_post_draw_steps()` is deferred. A callback or `frame_post_draw` signal is not a
general promise that arbitrary GPU readback has completed. Use the resource/device
operation's synchronization contract for that work.

Sources: [server frame and main loop](doc_rendering_source_map#startup-and-dispatch),
[viewports](doc_rendering_source_map#viewports-and-scenes), and [device
submission](doc_rendering_source_map#resources-and-device).
