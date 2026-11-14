# Particles and deformation

Geometry can change after its scene instance is created. Particle simulation,
multimeshes, skeletons, blend shapes, and physics-driven deformation connect simulation
data to renderer storage and surface drawing.

## Particle paths

GPU particle nodes create particle resources through [RenderingServer](class_RenderingServer). Particle storage
retains simulation parameters, process materials, emission state, collision
relationships, draw-pass meshes, and GPU buffers.

RD particle storage runs simulation through its particle shader/compute path. GLES3
particle storage uses transform feedback. CPU particle nodes instead simulate outside
that GPU storage path and submit their resulting instance data through rendering
resources. These mechanisms share visual concepts without sharing one simulation
implementation.

The server frame updates scene/collider state before calling `update_particles()`. The
scene culler's update also prepares particle colliders. This order lets simulation
consume current collision and transform inputs before the viewport renders particle
geometry.

## Drawing particle geometry

Particle simulation output feeds draw meshes and instance data. Scene culling must
retain particle bounds and process-material dependencies. Concrete canvas or scene
renderers interpret the buffers and select the appropriate draw path. Simulation and
visible drawing are therefore separate operations.

Collision volumes, heightfield collision rendering, trails, sorting, and sub-emission
add dependencies beyond a single compute dispatch. A change to particle-buffer layout
must update simulation shaders and every draw consumer of that layout.

## Mesh instances and deformation

Mesh storage owns surface geometry, multimesh instance data, skeleton data, and
mesh-instance deformation state. Concrete geometry instances reference those resources
and cache surface/shader information for rendering.

RD dirty-resource processing updates global shader uniforms, queued materials, dirty
multimeshes, dirty skeletons, and the decal atlas in a defined order. Instance
dependency changes can queue that work. Reordering updates solely by their storage class
names can break a frame's data dependencies.

Bounds are another contract. CPU culling uses a representation of the geometry's
possible extent; GPU-deformed positions do not automatically update that representation.
Custom geometry/deformation work must preserve bounds and visibility behavior as well as
buffer contents.

## Physics crossing

Physics soft-body integration uses `PhysicsServer3DRenderingServerHandler` to write
rendering mesh data. That is a renderer-facing contract outside the renderer directory.
Renaming surface layouts or moving deformation ownership requires checking physics
implementations and the scene soft-body handler, not only mesh storage.

## Change route

For a new deformation feature, trace scene configuration, storage data layout, dirty
notifications, bounds, previous/current transform or vertex data, simulation/deformation
shaders, and final draw consumers. Temporal effects and motion vectors can require
previous state as well as the current result.

Sources: [particle simulation and dirty
updates](doc_rendering_source_map#shaders-and-effects), [mesh/particle
storage](doc_rendering_classes_rd_storage), [GLES3 storage](doc_rendering_classes_gles),
and [physics integration](doc_rendering_integration_sources#services-and-modules).
