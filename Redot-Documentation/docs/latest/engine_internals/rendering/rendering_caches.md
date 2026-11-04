# Shader and pipeline caches

Rendering caches operate at different levels. Some reuse live RD objects; others persist
compilation data. Their invalidation and lifetime contracts cannot be replaced by one
generic map of shader names.

| Cache/helper | Key information | Lifetime or invalidation contract |
|---|---|---|
| [UniformSetCacheRD](doc_rendering_classes_rd) | Shader, set index, uniform types/bindings/resource RIDs | RD invalidation callback removes dead entries |
| [FramebufferCacheRD](doc_rendering_classes_rd) | Attachment textures, pass descriptions, view count | RD framebuffer/resource invalidation removes entries |
| [PipelineCacheRD](doc_rendering_classes_rd) | Configured pipeline state and requested formats | Owns/reuses RD pipeline objects for those combinations |
| [PipelineHashMapRD](doc_rendering_classes_rd) | Method-specific pipeline keys/hashes | Tracks queued compilation, completed pipelines, and worker tasks |
| [PipelineDeferredRD](doc_rendering_classes_rd) | One set of graphics/compute creation parameters | Waits for its task; requires explicit freeing before shader destruction |
| `ShaderRD` cache | Template/variant/custom-code compilation inputs | Loads/saves compiled variant data; configuration controls compression/debug data |
| Native pipeline cache | Backend/device-specific compilation information | Driver support and cache identity determine persistence |

## Live resource caches

Uniform sets and framebuffers reference other RD resources. Caches register invalidation
callbacks so destruction of a dependency can remove cached objects. A cache lookup that
returns a RID therefore depends on the resource graph and callback order, not only a
matching hash.

Framebuffer compatibility includes pass and view information as well as attachment
identities. A pass or multisampling change can require different pipelines even when a
material shader has not changed.

## Background compilation

`PipelineHashMapRD` avoids resubmitting keys already scheduled for compilation. Workers
publish completed pipelines into a queue that is merged into the map. Mutexes protect
task and completion state. Source counters distinguish canvas, mesh, surface, draw, and
specialization compilation origins.

`PipelineDeferredRD` stores creation parameters and a worker task. Its destructor waits
for task completion; a development assertion requires the pipeline to have been freed
manually before destruction and before the corresponding shader is freed. A wrapper
replacement must preserve that order.

## Persistent compilation data

The RD compositor configures the shader cache, attempts to create a writable cache
directory, and also recognizes exported project shader-cache data. The editor forces
shader caching on. Runtime settings control compression, compression method, and
retained debug information.

RD also maintains a native pipeline-cache path through the driver interface. Vulkan
implements native cache creation/query/serialization and uses a device/driver-related
cache identity.

The D3D12 implementation warns that PSO caching is unimplemented and returns false from
cache creation. Metal's cache-creation function returns false before the binary-archive
setup code. Neither case means that all shader caching is absent: `ShaderRD` bytecode
caching and live pipeline reuse are separate layers.

## Refactoring implications

Consolidating the cache interfaces could improve consistency, but only after separating
object reuse, task scheduling, and persistent native compilation data. A shared name for
those concerns would hide their different contracts.

For startup stutter, measure cache hits/misses, compilation source counters, foreground
waits, and driver work separately. Rebuilding a cache key or a pipeline too often is a
different problem from a missing native persistent cache. See [performance
proposals](doc_rendering_performance).

Sources: [cache and shader classes](doc_rendering_classes_rd), [shader/pipeline entry
points](doc_rendering_source_map#shaders-and-effects), and [backend cache
gaps](doc_rendering_observation_sources#incomplete-and-disabled-paths).
