# Renderer dependency map

This map describes contracts that cross the rendering boundary. An include, runtime
call, callback, inherited interface, and borrowed resource each create different
obligations for decoupling.

<mermaid ref="./diagrams/rendering-dependencies.mmd"
         alt="Renderer dependencies cross core, scene resources, modules, platform code, XR, camera, and native effect integration." />

## Dependencies into rendering

| Consumer | Provider | Mechanism and contract |
|---|---|---|
| Scene nodes/resources | Rendering server and retained state | Calls/RIDs; preserve object identity, notifications, and teardown |
| Core resource loader | Rendering server | Queue progress through `sync()` in single-thread rendering; preserve loading progress |
| Core `Engine` | Main RD | `set_max_fps()` forwards the limit into RD; preserve frame-pacing behavior |
| Editor previews/importers | Server, resources, shaders, RD tools | Offscreen outputs, shader metadata, GPU work, and readback |
| Lightmapper module | Local RD | Independent submission, shader compilation, and resource ownership |
| Betsy compressor | Server, RD, concrete contexts | Local-device creation and fallback context construction |
| XR/OpenXR | Server outputs and native drivers | Creation hooks, borrowed textures, view counts, runtime frame timing |
| Camera feeds | Server texture API/native handles | Asynchronous image updates and feed lifetime |
| Physics soft bodies | Mesh/rendering handler | Update geometry data without invalidating layout/lifetime contracts |
| Text, debug views, navigation | Textures, meshes, canvas/server APIs | Rendering consumers outside the scene renderer |
| Movie capture | Viewport output and readback | Completed output image, format conversion, timing data |

## Dependencies out of rendering

| Renderer component | External provider | Mechanism and consequence |
|---|---|---|
| Server dispatch and culling | Core objects, templates, queues, threads, math | Shared allocation, tasks, synchronization, bounds, and object/callable semantics |
| Shader language/preprocessor | Scene `ShaderInclude` | Header dependency on a resource-layer type |
| RD source compilation | glslang module | Conditional direct call to compilation interface |
| Compositor/viewport/rendering method | XR server/interface | Frame hooks, output overrides, view transforms and dimensions |
| GLES3 scene environment | Camera server/feed | Camera-image background consumption |
| Context/driver interfaces | Display server types | Window IDs, VSync modes, surface lifecycle |
| Vulkan driver | Android platform glue | Conditional OS/Java/thread integration |
| MetalFX effect | Concrete Metal driver and native objects | Driver casts, native handles, command-buffer callback and completion lifetime |

## Internal crossings that matter

Within rendering, `RSG` and concrete singleton access connect storage, cullers, scene
renderers, and viewports. RD texture storage asks the scene renderer for an HDR format.
RD material storage inspects concrete scene shader-data types. GI exposes structures
used directly by scene-render code.

Those dependencies are narrower than an engine-wide transitive graph, but they affect
extraction of a subsystem. Moving only its public header will not remove its runtime
singleton, callback, or shared-resource requirements.

## How to use the indexes

The [consumer indexes](doc_rendering_reference) identify direct includes and selected
renderer symbol references across committed source. They include conditional and test
code. Indirect calls through resource objects or instance variables can require further
tracing; a missing search hit does not establish absence of a dependency.

For a change, start with its provider interface, inspect each consumer's use, then
follow ownership and callbacks one boundary further. Record which tests or runtime
scenarios exercise the contract. The [external-consumer guide](doc_rendering_consumers)
expands the most consequential cases; [boundary proposals](doc_rendering_boundaries)
separate suggested changes from these existing relationships.

Sources: [scene resources](doc_rendering_integration_sources#scene-resources),
[services/modules/core
crossings](doc_rendering_integration_sources#services-and-modules), and [internal
crossings](doc_rendering_observation_sources#wrapper-and-ownership-observations).
