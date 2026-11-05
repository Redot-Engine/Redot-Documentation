# Unfinished and disabled paths

Source markers identify investigation leads. They do not establish that code is
reachable, defective, or inexpensive to finish. This assessment separates confirmed
implementation status from work needed to judge its impact.

## Assessed examples

| Area | Existing behavior | Assessment and next check |
|---|---|---|
| D3D12 native PSO cache | Creation returns false; related methods report unimplemented operations | Persistent native cache gap; evaluate startup/pipeline workloads and cache compatibility |
| Metal native pipeline cache | Creation returns false before binary-archive setup | Disabled implementation path; determine why it is disabled before enabling it |
| Secondary command recording | RD requests zero secondary buffers; source cites unresolved vendor issues | Intentionally disabled optimization; Metal secondary methods are also unimplemented |
| Mobile post-opaque effect callback | Emits an unsupported warning | Feature limit tied to subpass organization; a callback insertion alone is insufficient |
| Mobile MSAA resolved depth for effects | Emits an unsupported warning | Attachment/resolve support gap; test callback inputs and native pass support |
| Metal explicit barrier method | Warns once and does no work | Requires analysis alongside tracked hazards and encoder transitions; not proof of broken normal synchronization |
| Metal copy via intermediate buffer | Rejects a copy when incompatible compressed format handling requires a temporary buffer | Conditional feature gap; reproduce the formats and copy regions before proposing implementation |
| Legacy `texture_bind` dispatch | Disabled under `#if 0` with a Vulkan-refactor marker | Historical inactive API path; audit callers and current replacement before restoring/removing it |
| GLES3 GLAD constructor failure | Returns through an error macro before later members initialize; source flags cleanup risk | Failure-lifetime concern; reproduce failed loading and inspect destruction behavior |
| Temporal accumulation reset | Source says the engine supplies no reset mechanism at that point | State/API limitation; define camera-cut, resize, and history reset semantics |

## Additional markers

The generated [source marker catalog](doc_rendering_reference) includes TODO/FIXME/HACK
lines and unimplemented diagnostics across the primary renderer and drivers. It also
includes shader markers about roughness/specular behavior and energy conservation, RD
buffer/view limitations, and GI/effect cleanup.

There are relevant markers outside that sweep. The source maps include the resource
loader's coupling TODO, and platform constructors can contain their own
fallback/support-check TODOs. Review external integration changes during each refresh
rather than limiting investigation to the primary directories.

The primary catalog counts matching lines, not independent tasks. One function can
produce several entries, and a multi-line comment can continue beyond its matching line.
Read the surrounding source and callers before assigning work.

## Classify before implementing

Use one of four outcomes: intentional capability limit, disabled working area, reachable
missing behavior, or stale/uncertain marker. Identify the build flags and runtime
conditions needed to reach the operation.

For a reachable gap, record the missing contract, fallback/error behavior, affected
methods/platforms, resource ownership, and regression scenario. For a disabled
optimization, preserve its historical reason until measurements and target hardware
establish that it can be enabled safely.

Do not group shader correctness, native capability support, and CPU overhead into one
cleanup task. They need different evidence and validation. The [performance
proposals](doc_rendering_performance) identify smaller measurement targets separately
from these larger backend projects.

Sources: [assessed incomplete
paths](doc_rendering_observation_sources#incomplete-and-disabled-paths), [disabled
recording
configuration](doc_rendering_observation_sources#wrapper-and-ownership-observations),
and [source marker indexes](doc_rendering_reference).
