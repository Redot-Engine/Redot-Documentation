# Rendering integration source map

These source entry points support the architecture pages. Their revision and review date
appear once in the [baseline](doc_rendering_baseline). The refresh tool relocates
symbols when line numbers change.

## Scene resources

- [CanvasItem draw delegation](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/main/canvas_item.cpp#L871)
- [Sprite2D draw notification](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/sprite_2d.cpp#L153)
- [Visual instance ownership](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/visual_instance_3d.cpp#L213)
- [Mesh instance mesh changes](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/mesh_instance_3d.cpp#L119)
- [Texture resource](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/resources/image_texture.h#L45)
- [Shader resource](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/resources/shader.h#L47)
- [Material resource](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/resources/material.h#L47)
- [RD texture bridge](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/resources/texture_rd.cpp#L96)
- [Compositor resource](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/resources/compositor.cpp#L108)
- [Mode7 material and texture generation](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/mode7_sprite_2d.cpp#L218)
- [WorldScape material](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/modules/worldscape_3d/worldscape_3d_material.h#L44)

## Services and modules

- [Display server contract](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/display/display_server.h#L51)
- [XR interface contract](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/xr/xr_interface.h#L58)
- [Camera feeds](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/camera/camera_feed.h#L50)
- [GL camera feed consumption](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/rasterizer_scene_gles3.cpp#L52)
- [OpenXR Vulkan hooks](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/modules/openxr/extensions/platform/openxr_vulkan_extension.h#L51)
- [OpenXR D3D12 hooks](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/modules/openxr/extensions/platform/openxr_d3d12_extension.h#L53)
- [GPU texture compressor](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/modules/betsy/image_compress_betsy.cpp#L90)
- [GPU lightmapper](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/modules/lightmapper_rd/lightmapper_rd.h#L47)
- [Raycast occlusion implementation](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/modules/raycast/raycast_occlusion_cull.h#L48)
- [GLSL to SPIR-V compiler](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/modules/glslang/register_types.cpp#L49)
- [Render server registration](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/register_server_types.cpp#L154)
- [Resource loader render progress dependency](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/core/io/resource_loader.cpp#L953)
- [Engine frame limit device dependency](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/core/config/engine.cpp#L95)
- [Physics soft body rendering contract](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/physics_3d/physics_server_3d.h#L224)
