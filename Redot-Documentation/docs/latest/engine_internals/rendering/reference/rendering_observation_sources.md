# Rendering observation source map

These source entry points support the architecture pages. Their revision and review date
appear once in the [baseline](doc_rendering_baseline). The refresh tool relocates
symbols when line numbers change.

## Wrapper and ownership observations

- [Scene dependency on shader includes](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/shader_language.h#L49)
- [Shader preprocessor scene dependency](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/shader_preprocessor.h#L48)
- [RD compiler module dependency](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device.cpp#L50)
- [Android Vulkan platform dependency](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/vulkan/rendering_device_driver_vulkan.cpp#L48)
- [Texture storage to scene renderer dependency](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/storage_rd/texture_storage.cpp#L4326)
- [Server free dispatch](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_server_default.cpp#L50)
- [Material storage inspects concrete scene shader types](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/storage_rd/material_storage.cpp#L2395)
- [Secondary command recording is disabled](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device.cpp#L140)
- [GI intended separation comment](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/environment/gi.h#L110)
- [Resource loader coupling TODO](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/core/io/resource_loader.cpp#L956)
- [Betsy private server include](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/modules/betsy/image_compress_betsy.h#L47)

## Incomplete and disabled paths

- [D3D12 pipeline cache stub](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/d3d12/rendering_device_driver_d3d12.cpp#L4446)
- [Metal pipeline cache early return](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L1770)
- [Metal secondary command buffer rejection](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L968)
- [Metal pipeline barrier placeholder](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L831)
- [Mobile compositor limitations](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/forward_mobile/render_forward_mobile.cpp#L857)
- [Legacy disabled texture binding](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_server_default.h#L234)
- [GLES3 constructor failure marker](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/rasterizer_gles3.cpp#L285)
- [Temporal accumulation reset marker](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/forward_clustered/render_forward_clustered.cpp#L2575)

## Performance investigation points

- [Viewport usage snapshot allocation](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_viewport.cpp#L923)
- [Viewport sibling lookup](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_viewport.cpp#L129)
- [Render target storage usage flag](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/storage_rd/texture_storage.cpp#L4338)
- [Uniform binding lookup](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device.cpp#L3643)
- [Staging capacity fallback](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device.cpp#L363)
- [FSR2 UBO allocation marker](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/effects/fsr2.cpp#L186)
