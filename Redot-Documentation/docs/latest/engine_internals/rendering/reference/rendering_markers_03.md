# Renderer source markers 3

These are source markers, not confirmed defects or approved work. Read the containing
function and its callers before classifying a marker. Some describe intentional API
limits or old comments. See [unfinished paths](doc_rendering_unfinished) for assessed
examples. The [maintenance procedure](doc_rendering_maintenance) rebuilds this catalog.

| Source | Marker text |
|---|---|
| [drivers/metal/metal_objects.mm:899](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L899) | ERR_FAIL_MSG("not implemented"); |
| [drivers/metal/metal_objects.mm:922](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L922) | ERR_FAIL_MSG("not implemented"); |
| [drivers/metal/metal_objects.mm:1317](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L1317) | ERR_PRINT("not implemented: UNIFORM_TYPE_TEXTURE_BUFFER"); |
| [drivers/metal/metal_objects.mm:1320](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L1320) | ERR_PRINT("not implemented: UNIFORM_TYPE_SAMPLER_WITH_TEXTURE_BUFFER"); |
| [drivers/metal/metal_objects.mm:1323](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L1323) | CRASH_NOW_MSG("not implemented: UNIFORM_TYPE_IMAGE_BUFFER"); |
| [drivers/metal/metal_objects.mm:1486](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L1486) | ERR_PRINT("not implemented: UNIFORM_TYPE_TEXTURE_BUFFER"); |
| [drivers/metal/metal_objects.mm:1489](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L1489) | ERR_PRINT("not implemented: UNIFORM_TYPE_SAMPLER_WITH_TEXTURE_BUFFER"); |
| [drivers/metal/metal_objects.mm:1492](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L1492) | CRASH_NOW_MSG("not implemented: UNIFORM_TYPE_IMAGE_BUFFER"); |
| [drivers/metal/metal_objects.mm:1643](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L1643) | ERR_PRINT("not implemented: UNIFORM_TYPE_TEXTURE_BUFFER"); |
| [drivers/metal/metal_objects.mm:1646](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L1646) | ERR_PRINT("not implemented: UNIFORM_TYPE_SAMPLER_WITH_TEXTURE_BUFFER"); |
| [drivers/metal/metal_objects.mm:1649](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L1649) | CRASH_NOW_MSG("not implemented: UNIFORM_TYPE_IMAGE_BUFFER"); |
| [drivers/metal/metal_objects.mm:2136](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L2136) | CRASH_NOW_MSG("Not implemented"); |
| [drivers/metal/metal_objects.mm:2141](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L2141) | CRASH_NOW_MSG("Not implemented"); |
| [drivers/metal/rendering_device_driver_metal.mm:233](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L233) | // TODO(sgc): Evaluate lossy texture support (perhaps as a project option?) |
| [drivers/metal/rendering_device_driver_metal.mm:426](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L426) | // TODO(sgc): is it ok to create a shared texture from a multi-sample texture? |
| [drivers/metal/rendering_device_driver_metal.mm:631](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L631) | // TODO: Confirm with rendering team that there is no other way Redot may attempt to map a texture with multiple mipmaps or array layers. |
| [drivers/metal/rendering_device_driver_metal.mm:838](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L838) | WARN_PRINT_ONCE("not implemented"); |
| [drivers/metal/rendering_device_driver_metal.mm:969](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L969) | ERR_FAIL_V_MSG(false, "not implemented"); |
| [drivers/metal/rendering_device_driver_metal.mm:978](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L978) | ERR_FAIL_MSG("not implemented"); |
| [drivers/metal/rendering_device_driver_metal.mm:1362](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L1362) | // TODO. |
| [drivers/metal/rendering_device_driver_metal.mm:1370](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L1370) | //p_linear_pool_index = -1; // TODO:? Linear pools not implemented or not supported by API backend. |
| [drivers/metal/rendering_device_driver_metal.mm:1459](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L1459) | ERR_FAIL_MSG("not implemented: copy with intermediate buffer"); |
| [drivers/metal/rendering_device_driver_metal.mm:2058](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L2058) | uint32_t alignment = 16; // TODO(sgc): is this always correct? |
| [drivers/metal/rendering_device_driver_metal.mm:2175](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/rendering_device_driver_metal.mm#L2175) | ERR_FAIL_V_MSG(PipelineID(), "tessellation not implemented"); |
