# Renderer execution source map

These source entry points support the architecture pages. Their revision and review date
appear once in the [baseline](doc_rendering_baseline). The refresh tool relocates
symbols when line numbers change.

## Startup and dispatch

- [Engine renderer selection](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/main/main.cpp#L2429)
- [Main::setup2](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/main/main.cpp#L3011)
- [Main loop render synchronization](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/main/main.cpp#L5003)
- [RenderingServerDefault::_init](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_server_default.cpp#L222)
- [RenderingServerDefault::_draw](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_server_default.cpp#L74)
- [RenderingServerDefault::init](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_server_default.cpp#L258)
- [RenderingServerDefault::sync](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_server_default.cpp#L417)
- [RenderingServerDefault::_finish](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_server_default.cpp#L244)
- [Texture dispatch wrappers](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_server_default.h#L143)
- [Server threading macros](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/server_wrap_mt_common.h#L198)
- [Renderer globals](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_server_globals.h#L58)
- [Compositor construction and method selection](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/renderer_compositor_rd.cpp#L324)
- [Dummy compositor](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/dummy/rasterizer_dummy.h#L56)

## Viewports and scenes

- [Viewport drawing and consumer tracking](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_viewport.cpp#L827)
- [Viewport sorting](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_viewport.cpp#L177)
- [Viewport composition](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_viewport.cpp#L415)
- [Canvas command retention](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_canvas_cull.cpp#L1533)
- [Canvas culling and dispatch](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_canvas_cull.cpp#L78)
- [RD canvas execution](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/renderer_canvas_render_rd.cpp#L515)
- [Camera and scene culling](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_scene_cull.cpp#L2595)
- [Scene preparation and render handoff](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_scene_cull.cpp#L3118)
- [Dirty instance dependency updates](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_scene_cull.cpp#L3978)
- [Shared RD scene setup](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/renderer_scene_render_rd.cpp#L1322)
- [Forward+ pass orchestration](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/forward_clustered/render_forward_clustered.cpp#L1737)
- [Mobile pass orchestration](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/forward_mobile/render_forward_mobile.cpp#L796)
- [GLES3 pass orchestration](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/drivers/gles3/rasterizer_scene_gles3.cpp#L2244)

## Resources and device

- [Storage dependency tracking](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/storage/utilities.h#L74)
- [RD texture storage](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/storage_rd/texture_storage.h#L54)
- [RD texture deletion](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/storage_rd/texture_storage.cpp#L756)
- [Render scene buffers](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/storage_rd/render_scene_buffers_rd.h#L71)
- [RenderingDevice initialization](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device.cpp#L6757)
- [RenderingDevice texture creation](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device.cpp#L873)
- [RenderingDevice uniform validation](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device.cpp#L3626)
- [RenderingDevice resource deletion](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device.cpp#L6147)
- [Frame resource retirement](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device.cpp#L6506)
- [Device submission and presentation](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device.cpp#L6371)
- [Local device submission](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device.cpp#L6383)
- [Device command graph](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device_graph.cpp#L2254)
- [Context contract](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_context_driver.h#L45)
- [Driver contract](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device_driver.h#L94)

## Shaders and effects

- [Shader preprocessing](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/shader_preprocessor.h#L50)
- [Shader language](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/shader_language.h#L55)
- [Shader compiler](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/shader_compiler.h#L45)
- [RD shader template and variants](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/shader_rd.h#L49)
- [Shader containers](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_shader_container.h#L44)
- [Generated shader wrapper builder](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/glsl_builders.py#L94)
- [Uniform set cache](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/uniform_set_cache_rd.h#L46)
- [Pipeline background compilation](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/pipeline_hash_map_rd.h#L47)
- [Post processing and tone mapping](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/renderer_scene_render_rd.cpp#L455)
- [Compositor callback execution](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/renderer_scene_render_rd.cpp#L304)
- [Particle simulation](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/storage_rd/particles_storage.cpp#L1425)
- [Shader container reflection and native code](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device.cpp#L3423)
- [Cluster construction](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/cluster_builder_rd.cpp#L494)
- [RD storage dirty-resource order](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/storage_rd/utilities.cpp#L259)
- [MetalFX graph callback and native encoding](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/effects/metal_fx.mm#L84)
- [RenderingServer deprecated adapters](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_server.compat.inc#L33)
- [RenderingDevice deprecated adapters](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/rendering_device.compat.inc#L33)
- [RD render buffer deprecated adapters](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/servers/rendering/renderer_rd/storage_rd/render_scene_buffers_rd.compat.inc#L33)
