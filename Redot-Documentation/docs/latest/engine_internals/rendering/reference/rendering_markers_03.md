# Renderer source markers 3

These are source markers, not confirmed defects or approved work. Read the containing
function and its callers before classifying a marker. Some describe intentional API
limits or old comments. See [unfinished paths](doc_rendering_unfinished) for assessed
examples. The [maintenance procedure](doc_rendering_maintenance) rebuilds this catalog.

| Source | Marker text |
|---|---|
| [drivers/gles3/storage/particles_storage.cpp:1135](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/storage/particles_storage.cpp#L1135) | while (todo &gt;= frame_time) { |
| [drivers/gles3/storage/particles_storage.cpp:1137](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/storage/particles_storage.cpp#L1137) | todo -= frame_time; |
| [drivers/gles3/storage/particles_storage.cpp:1140](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/storage/particles_storage.cpp#L1140) | particles-&gt;frame_remainder = todo; |
| [drivers/gles3/storage/particles_storage.cpp:1155](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/storage/particles_storage.cpp#L1155) | double todo = particles-&gt;request_process_time; |
| [drivers/gles3/storage/particles_storage.cpp:1156](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/storage/particles_storage.cpp#L1156) | while (todo &gt;= 0) { |
| [drivers/gles3/storage/particles_storage.cpp:1158](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/storage/particles_storage.cpp#L1158) | todo -= frame_time; |
| [drivers/gles3/storage/texture_storage.cpp:2032](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/storage/texture_storage.cpp#L2032) | /// @todo Validate texture atlas size with maximum texture size |
| [drivers/gles3/storage/texture_storage.cpp:2490](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/storage/texture_storage.cpp#L2490) | /// @todo Convert to Size2i internally |
| [drivers/metal/metal_objects.mm:641](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L641) | CRASH_COND_MSG(!flags::all(pf.getCapabilities(resolve_tex.pixelFormat), kMTLFmtCapsResolve), "not implemented: unresolvable texture types"); |
| [drivers/metal/metal_objects.mm:733](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/metal/metal_objects.mm#L733) | CRASH_NOW_MSG("unimplemented: using a texture format that is not supported for resolve"); |
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
