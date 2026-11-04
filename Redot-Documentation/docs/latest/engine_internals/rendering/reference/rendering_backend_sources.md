# Rendering backend source map

These source entry points support the architecture pages. Their revision and review date
appear once in the [baseline](doc_rendering_baseline). The refresh tool relocates
symbols when line numbers change.

## Vulkan

- [Vulkan context](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/vulkan/rendering_context_driver_vulkan.h#L52)
- [Vulkan device initialization](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/vulkan/rendering_device_driver_vulkan.cpp#L1530)
- [Vulkan allocator](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/vulkan/rendering_device_driver_vulkan.cpp#L1264)
- [Vulkan shader container](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/vulkan/rendering_shader_container_vulkan.cpp#L55)
- [Vulkan interop hooks](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/vulkan/vulkan_hooks.h#L43)

## Direct3D 12

- [D3D12 context](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/d3d12/rendering_context_driver_d3d12.h#L81)
- [D3D12 device](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/d3d12/rendering_device_driver_d3d12.h#L91)
- [D3D12 shader conversion](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/d3d12/rendering_shader_container_d3d12.h#L81)
- [D3D12 descriptor and resource implementation](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/d3d12/rendering_device_driver_d3d12.cpp#L1023)
- [D3D12 interop hooks](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/d3d12/d3d12_hooks.h#L43)

## Metal

- [Metal context](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_context_driver_metal.h#L66)
- [Metal device](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.h#L57)
- [Metal shader container](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_shader_container_metal.h#L124)
- [Metal command buffer wrappers](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.h#L124)
- [Metal device format features](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_device_properties.h#L75)

## OpenGL and platforms

- [GLES3 compositor](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/rasterizer_gles3.cpp#L128)
- [GLES3 shader manager](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/shader_gles3.h#L53)
- [GLES3 canvas execution](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/rasterizer_canvas_gles3.cpp#L115)
- [EGL manager](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/egl/egl_manager.h#L49)
- [X11 context and compositor selection](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/platform/linuxbsd/x11/display_server_x11.cpp#L7208)
- [Wayland rendering integration](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/platform/linuxbsd/wayland/display_server_wayland.h#L71)
- [Windows context selection](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/platform/windows/display_server_windows.cpp#L6979)
- [macOS context selection](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/platform/macos/display_server_macos.mm#L3878)
- [Apple embedded context selection](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/apple_embedded/display_server_apple_embedded.mm#L94)
- [Android rendering integration](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/platform/android/display_server_android.h#L42)
- [WebGL context creation](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/platform/web/display_server_web.cpp#L1129)
- [Headless selection](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/display/display_server_headless.h#L44)
