# Performance investigation proposals

These are source-backed candidates, not measured improvements. Prioritize small
experiments that preserve output and synchronization. Measure CPU cost, GPU cost,
allocations, and compilation separately; fewer wrappers do not imply a faster frame.

## Smaller experiments

| Proposal | Source observation | Validation before accepting |
|---|---|---|
| Retain viewport snapshot scratch capacity | `draw_viewports()` creates/resizes a local `LocalVector<bool>` each call despite a comment about avoiding per-frame allocation | Count allocations across frames; compare member/thread-local reuse under viewport add/remove and shutdown |
| Index sibling RIDs during viewport sorting | Sibling graph construction searches the sibling list for each consumer RID | Compare sort time for many siblings; preserve order and cycle fallback |
| Reduce viewport usage discovery scans | Each drawn viewport snapshots and rechecks active target usage | Measure cost versus active/drawn viewport counts; preserve newly observed consumer relationships and update modes |
| Narrow render-target storage flags | Texture storage's TODO says the storage-image bit should be requested only when FSR requires it | Audit every storage-image consumer, including native/optional effects; compare format support, memory, and GPU timings |
| Improve uniform binding lookup if significant | Uniform-set construction searches supplied uniforms for each reflected binding | Measure creation frequency and typical binding counts; compare indexed lookup without changing validation/errors |

The first two candidates have relatively contained CPU data-structure changes. The
storage-flag candidate can affect image capabilities and native allocation, so a short
diff does not make it a low-risk GPU change.

## Trace before changing

Viewport texture-use discovery feeds later ordering. It clears consumer state and
records observed transitions, and a target stores one consumer identity. Before
optimizing the scans, test first-frame behavior, multiple consumers, sibling cycles,
visibility modes, and disappearing dependencies. These cases can expose semantics that a
faster lookup alone does not solve.

Uniform sets are often cached. An apparently quadratic creation loop may have little
frame cost when it executes only during initialization or invalidation. Measure cache
hit/miss rates before adding persistent lookup structures.

## Larger investigations

- RD staging pressure can force frame stalls. The source mentions blocking a
  loading thread until staging space becomes available as future work. That
  needs a progress/backpressure design and deadlock testing, not only a larger
  buffer setting.
- FSR2 has a TODO about combining UBO allocations if RD could expose views into
  a memory range. Such a change affects API, alignment, binding, and lifetime.
- Native D3D12/Metal persistent pipeline caches have gaps. Study compilation
  workloads and compatibility before treating them as quick fixes.
- Secondary command recording is intentionally disabled because of unresolved
  vendor behavior. Enabling it is a hardware validation project.

## Measurement recipe

Use a fixed revision, build configuration, driver, GPU, scene, and output size. Capture
warm and cold shader-cache runs separately. Compare one proposed change at a time,
retain a baseline distribution of frame times, and account for CPU/GPU overlap instead
of summing unrelated timers.

Use existing renderer timestamps, viewport measurements, pipeline compilation source
counters, memory reports where supported, and CPU allocation profiling. Include scenes
that stress the proposed path as well as ordinary scenes.

For a wrapper reduction, record both navigation/maintenance benefit and any measured
runtime effect. If the wrapper carries thread or lifetime behavior, validation must
establish correctness before comparing timings.

Sources: [performance investigation
points](doc_rendering_observation_sources#performance-investigation-points), [cache
gaps](doc_rendering_observation_sources#incomplete-and-disabled-paths), and [cache
contracts](doc_rendering_caches).
