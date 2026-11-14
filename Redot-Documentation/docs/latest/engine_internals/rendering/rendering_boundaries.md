# Boundary observations and proposals

The following observations describe existing dependencies. Proposals identify possible
changes; they are not descriptions of the present architecture or approved designs. A
directory or abstract base class alone does not establish an enforced boundary.

## Verified observations

| ID | Observation | Consequence |
|---|---|---|
| B1 | `RSG` publishes collaborators through mutable static pointers | Dependencies and setup order are implicit at many call sites |
| B2 | Server macros obtain their destination from surrounding definitions | Thread and owner behavior are difficult to follow from one declaration |
| B3 | Shader language/preprocessor headers include scene `ShaderInclude` | Lower-level shader tooling depends on the scene resource layer |
| B4 | RD conditionally calls the glslang module directly | Core device source compilation depends on a module interface |
| B5 | RD texture storage asks the concrete scene renderer for HDR format | Resource storage depends on rendering-method policy |
| B6 | RD material storage casts shader data to concrete Forward+/Mobile types | Storage behavior knows scene implementation types |
| B7 | GI data structures are used directly by scene render code | GI extraction must account for shared layouts and mutation |
| B8 | Core resource loading pumps the rendering server | Loading progress depends on renderer execution policy |
| B9 | MetalFX uses concrete Metal types and native command callbacks | Effects can cross the RD abstraction and native lifetime boundary |
| B10 | Vulkan has conditional Android glue; context APIs use display types | Native drivers/context contracts are not wholly platform-neutral |
| B11 | Scene pass selection remains in concrete renderer conditionals | New stages require coordinated edits to policy, buffers, and bindings |

## Evidence of intended separation

The GI header explicitly notes that [VoxelGIInstance](doc_rendering_classes_effects) remains directly used by render
code and should be addressed during refactoring. The resource loader explicitly proposes
a future server-progress mechanism with less coupling. The MetalFX source asks for an
API to retain objects until GPU completion. These are documented intentions, with no
implementation schedule implied.

The broader storage/drawing and context/device split is visible in interfaces and
construction. Treat a more strictly layered future design as an inference unless a
source comment or design record establishes it.

## Proposals in increasing scope

1. **Expose existing contracts.** Keep operation/owner maps and replace private
   implementation includes with narrower public includes where verified. For
   example, investigate Betsy's inclusion of the default-server header when
   its calls use the public server API. Validate relevant build combinations.
2. **Make selected dependencies explicit.** Supply small capability or format
   descriptions to texture/material consumers instead of concrete scene-renderer
   lookups. Start with B5/B6 and preserve behavior in both RD methods.
3. **Separate source resolution from scene resources.** Give shader tooling an
   include-resolution contract that scene resources and tools can implement.
   Preserve dependency reporting, file identity, and editor diagnostics.
4. **Define server progress and completion contracts.** Generalize the loader's
   progress dependency without replacing CPU queue completion with GPU waits.
   Cover single/separate-thread loading and shutdown before removing B8.
5. **Extract semantic pass planning.** Describe stage inputs/outputs, required
   attachments, callbacks, resolves, history, and ordering above RD. Preserve
   Mobile subpass decisions and probe/XR paths. This is a substantial redesign,
   separate from the existing device graph.

## How to assess a proposal

For each candidate, list current callers, owner/thread rules, represented data,
feature/build variants, and expected behavior changes. Demonstrate that a smaller
interface can express those obligations before moving implementation.

Begin with a narrow vertical slice and compare rendered output, resource lifetime, queue
progress, and relevant compilation counters. A reduction in wrapper count can improve
maintainability without producing a measurable frame time change; report those outcomes
separately.

Sources: [internal
crossings](doc_rendering_observation_sources#wrapper-and-ownership-observations),
[external services](doc_rendering_integration_sources#services-and-modules), [GI
classes](doc_rendering_classes_effects), and [wrapper chains](doc_rendering_wrappers).
