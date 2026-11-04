# Rendering internals

Redot's renderer keeps a representation of the scene, chooses visible work, produces GPU
commands, and presents images. Those jobs span scene resources, servers, renderer
implementations, device drivers, and platform code. A change to one layer can affect
resource lifetimes or execution order in another.

This manual describes those connections for maintainers who know C++ but have not worked
on the renderer. The [source baseline](doc_rendering_baseline) records the engine
revision and review date once for the whole manual. GDExtension and redot-cpp are
outside its scope.

## Start here

1. Read the [architecture](doc_rendering_architecture) and
   [vocabulary](doc_rendering_vocabulary).
2. Follow [startup](doc_rendering_startup), a [frame](doc_rendering_frame), and
   the [threading model](doc_rendering_threads).
3. Follow a [2D draw](doc_rendering_canvas) or a
   [3D instance](doc_rendering_scene).
4. Compare the [rendering methods](doc_rendering_methods) before entering a
   backend implementation.

## Follow the resources

- [Ownership and handles](doc_rendering_ownership) distinguish scene objects,
  server RIDs, device RIDs, and native handles.
- [Storage and invalidation](doc_rendering_storage) explain cached state and
  dependency notifications.
- [Textures and transfers](doc_rendering_textures) trace uploads, views, and
  the texture bridge.
- [Viewports and buffers](doc_rendering_viewports) explain output ordering,
  render targets, and intermediate images.
- [Materials and shaders](doc_rendering_shaders) follow compilation and
  [pipeline caches](doc_rendering_caches).

## Follow execution

- [Forward+](doc_rendering_forward_plus), [Mobile](doc_rendering_mobile), and
  [Compatibility](doc_rendering_compatibility) have different pass structures.
- [Lighting and environments](doc_rendering_lighting),
  [particles and deformation](doc_rendering_particles), and
  [effects and compositors](doc_rendering_effects) cross several components.
- [RenderingDevice](doc_rendering_device) validates resources and records work.
  Its [command graph](doc_rendering_graph) tracks hazards and orders commands.
- [Vulkan](doc_rendering_vulkan), [Direct3D 12](doc_rendering_d3d12), and
  [Metal](doc_rendering_metal) translate that work to native APIs.
- [Platform integration](doc_rendering_platforms) supplies windows, surfaces,
  contexts, and presentation support.

## Assess a change

The [dependency map](doc_rendering_dependencies), [external
consumers](doc_rendering_consumers), and [wrapper chains](doc_rendering_wrappers)
identify contracts to preserve. [Change routes](doc_rendering_change_routes) show where
a feature must travel.

[Boundary observations](doc_rendering_boundaries), [unfinished
paths](doc_rendering_unfinished), and [performance proposals](doc_rendering_performance)
distinguish existing behavior from suggested work. Proposals are investigation
candidates, not approved designs or measured speedups.

## Locate and update source

The [source reference](doc_rendering_reference) indexes every primary rendering header,
its declarations, and its companion implementation. It also contains source locators,
external reference searches, and a catalog of source markers.

The [build map](doc_rendering_build) explains conditional and generated code. The
[maintenance procedure](doc_rendering_maintenance) describes quarterly refreshes and the
review needed after regeneration.
