# Platform graphics integration

Platform code creates windows and the graphics objects associated with them. This layer
is part of rendering initialization, presentation, fallback, and shutdown even though it
lives outside the renderer directory.

## Integration map

| Platform path | Main integration points |
|---|---|
| Linux/BSD X11 | `DisplayServerX11`, Vulkan X11 context, GLX/EGL managers, native window events |
| Linux/BSD Wayland | `DisplayServerWayland`, Vulkan Wayland context, EGL/GLES managers, surface lifecycle |
| Windows | `DisplayServerWindows`, Vulkan Windows/D3D12 contexts, native GL and ANGLE managers |
| macOS | `DisplayServerMacOS`, Vulkan/Metal contexts, legacy GL and ANGLE managers, layer/window integration |
| Apple embedded | Shared Apple embedded display/context code used by platform display servers; Vulkan and Metal routes where built |
| Android | Android display server, Vulkan Android context, GL/Java/native-window and lifecycle integration |
| Web | `DisplayServerWeb`, Emscripten WebGL 2 context, browser/JavaScript lifecycle |
| Headless | `DisplayServerHeadless`, dummy compositor factory |

## Context versus device

For RD, the display server initializes a context driver and creates a window/surface
before initializing the main device. Context window operations update size and VSync,
and device screen operations manage presentation resources associated with a window.

Destroying a window therefore involves both device and context cleanup. A surface that
becomes minimized or temporarily unavailable can require skipped screen drawing or
swapchain recreation without destroying the retained scene.

For GL, context managers provide the current context, function loading, window binding,
and buffer swaps. The renderer relies on context affinity rather than RD's
command-buffer submission model.

## Selection and fallback

`Main` validates requested method/driver combinations and sets OS selection state.
Platform constructors contain further support checks and fallbacks. Windows has explicit
Vulkan/D3D12/OpenGL and native/ANGLE selection logic. macOS, Android, X11, Wayland, and
embedded Apple paths have different choices.

Fallback is constrained by compile flags and available libraries as well as settings. A
driver recognized by the common API is not necessarily compiled into a given binary.
Follow the target display constructor before documenting a platform's guaranteed
behavior.

## Translation layers

MoltenVK sits below the Vulkan API path. Native Metal uses Redot's Metal context/device
implementation instead. ANGLE sits below the GL-family context path. EGL is
context/surface integration for GL-family APIs, not a scene rendering method or an RD
device driver.

Web creates a WebGL 2 context and selects the GLES3 compositor. Browser context
availability/loss and JavaScript callbacks are part of that route. There is no WebGPU
driver in this snapshot's selection path.

## Cross-boundary behavior

The RD boot-image path pumps display resize events and may redraw after the window size
changes. This is an example of renderer code coordinating with platform events, rather
than presentation being a single isolated final call.

XR adds another output contract: its runtime may own images and frame timing instead of
an ordinary window swapchain. That integration must coexist with mirror windows,
resizing, VSync, and device creation hooks.

Sources: [platform selection entry
points](doc_rendering_backend_sources#opengl-and-platforms), [context
contract](doc_rendering_source_map#resources-and-device), and [boot/compositor
implementation](doc_rendering_source_map#startup-and-dispatch).
