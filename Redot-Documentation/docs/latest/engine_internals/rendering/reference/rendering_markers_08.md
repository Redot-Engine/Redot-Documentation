# Renderer source markers 8

These are source markers, not confirmed defects or approved work. Read the containing
function and its callers before classifying a marker. Some describe intentional API
limits or old comments. See [unfinished paths](doc_rendering_unfinished) for assessed
examples. The [maintenance procedure](doc_rendering_maintenance) rebuilds this catalog.

| Source | Marker text |
|---|---|
| [servers/rendering/shader_language.cpp:4936](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/shader_language.cpp#L4936) | /// @todo: Handle vector pairs? |
| [servers/rendering/shader_language.cpp:4945](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/shader_language.cpp#L4945) | /// @todo: Handle vector pairs? |
| [servers/rendering/shader_language.cpp:5074](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/shader_language.cpp#L5074) | /// @todo FIXME: Implement this. |
| [servers/rendering/shader_language.cpp:7161](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/shader_language.cpp#L7161) | /// @todo |
| [servers/rendering/shader_language.cpp:7166](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/shader_language.cpp#L7166) | /// @todo |
| [servers/rendering/shader_language.cpp:9501](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/shader_language.cpp#L9501) | _set_error(vformat(RTR("Uniform instances are not yet implemented for '%s' shaders."), shader_type_identifier)); |
| [servers/rendering/shader_types.cpp:196](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/shader_types.cpp#L196) | shader_modes[RS::SHADER_SPATIAL].functions["fragment"].built_ins["FOG"] = ShaderLanguage::TYPE_VEC4; /// @todo Consider adding to light shader |
| [servers/rendering/storage/mesh_storage.cpp:357](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/storage/mesh_storage.cpp#L357) | mmi-&gt;_data_interpolated = mmi-&gt;_data_curr; ///&lt; @todo Copy data rather than use Packed = function? |
| [servers/rendering/storage/mesh_storage.cpp:513](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/storage/mesh_storage.cpp#L513) | /// @todo Make sure AABBs are constantly up to date through the interpolation? |
| [servers/rendering/storage/render_scene_buffers.cpp:60](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/storage/render_scene_buffers.cpp#L60) | ADD_PROPERTY(PropertyInfo(Variant::INT, "scaling_3d_mode", PROPERTY_HINT_ENUM, "Bilinear (Fastest),FSR 1.0 (Fast),FSR 2.2 (Slow),MetalFX (Spatial),MetalFX (Temporal)"), "set_scaling_3d_mode", "get_scaling_3d_mode"); ///&lt; @todo VIEWPORT_SCALING_3D_MODE_OFF is possible here too, but we can't specify an enum string for it. |
| [servers/rendering/storage/texture_storage.h:157](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/storage/texture_storage.h#L157) | virtual void render_target_set_position(RID p_render_target, int p_x, int p_y) = 0; ///&lt; @todo Q change input to const Point2i &p_position ? |
| [servers/rendering/storage/texture_storage.h:159](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/storage/texture_storage.h#L159) | virtual void render_target_set_size(RID p_render_target, int p_width, int p_height, uint32_t p_view_count) = 0; ///&lt; @todo Q change input to const Size2i &p_size ? |
