# Renderer startup and shutdown

Renderer construction starts in engine and platform code, before the rendering server
creates its retained scene state. This page follows that order so that initialization
changes preserve context, thread, and resource lifetimes.

<mermaid ref="./diagrams/rendering-startup.mmd"
         alt="RD backends create a device, OpenGL establishes a context, and headless selects dummy rendering before the server creates its component family." />

## Select the implementation

`Main::setup()` registers rendering settings, reads command-line choices, and validates
method/driver combinations. Project settings and platform overrides select a default
when the command line does not specify one. Web's configured method is Compatibility.
Headless operation selects the dummy driver.

`Main::setup2()` creates the display server. A platform display server chooses and
initializes a [RenderingContextDriver](doc_rendering_classes_core) for an RD backend, creates the main window's
surface, and initializes a [RenderingDevice](class_RenderingDevice). It then calls
`RendererCompositorRD::make_current()` to install the compositor factory. For OpenGL,
platform context managers establish the context and select [RasterizerGLES3](doc_rendering_classes_gles). The
headless display server selects [RasterizerDummy](doc_rendering_classes_dummy).

Fallback policy lives in platform implementations as well as engine selection code.
Windows can try Vulkan, D3D12, and OpenGL according to compiled support and settings.
Apple, Android, X11, and Wayland paths differ. A failed context is therefore not handled
by one universal renderer fallback function.

## Create server components

`Main::setup2()` constructs [RenderingServerDefault](doc_rendering_classes_core) and calls `init()`. With a separate
render thread, `init()` releases the display server's current rendering thread, starts a
worker-pool pump task, assigns the server's thread identity, and queues `_init()`
synchronously. Without it, `_init()` runs on the main thread.

`_init()` creates [RendererCanvasCull](doc_rendering_classes_core), [RendererViewport](doc_rendering_classes_core), [RendererSceneCull](doc_rendering_classes_core), and
[RendererCameraAttributes](doc_rendering_classes_storage). The compositor factory constructs the selected component
family. The server publishes pointers in `RSG` and attaches the scene renderer to the
scene culler.

The RD compositor creates shared RD storage, cache, fog, and canvas objects. Its
constructor selects Mobile if the requested method is Mobile or the device supports
fewer than 48 textures per shader stage. A requested Forward+ path can consequently use
the Mobile implementation after this capability check. It initializes the scene
implementation after constructing the storage family.

## Device initialization

`RenderingDevice::initialize()` asks the context to create a device driver. It chooses a
physical device, considers presentation support for the requested surface, creates
graphics/compute, transfer, and presentation queues, and allocates frame command pools,
buffers, semaphores, fences, and timestamp data.

A device without a window uses one frame; the main surface path uses at least two. Queue
families can be shared when the device supports the required work. These resources exist
below the scene renderer and must outlive its GPU objects.

## Tear down in order

The main engine's rendering teardown calls `RenderingServer::finish()`.
`RenderingServerDefault::_finish()` finalizes and deletes server components and the
compositor. The RD compositor frees scene, canvas, storage, and effect resources before
the platform destroys the underlying device and context.

For threaded rendering, `finish()` queues teardown and pump exit, waits for the pump
task, and restores the server's main-thread identity. Device finalization handles
outstanding GPU work and native resources. Replacing this sequence requires preserving
both CPU task completion and GPU completion; they are different events.

Sources: [startup and dispatch](doc_rendering_source_map#startup-and-dispatch), [device
initialization](doc_rendering_source_map#resources-and-device), and [platform
implementations](doc_rendering_backend_sources#opengl-and-platforms).
