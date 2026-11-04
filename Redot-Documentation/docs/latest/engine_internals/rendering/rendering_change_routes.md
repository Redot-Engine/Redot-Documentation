# Routes for rendering changes

Use these routes to estimate a change's reach before editing. The paths name
responsibilities; some steps may be unnecessary for an internal-only change.

## Add a resource property

For a light, mesh, material, or texture property:

1. Identify the scene resource/node and its server RID.
2. Add or change the server contract and its dispatch destination.
3. Update storage interfaces and each applicable RD/GLES3/dummy implementation.
4. Preserve changed/deleted dependency notifications and dirty-state handling.
5. Update the concrete scene/canvas representation and shader parameters.
6. Check shadows, probes, capture, editor previews, and external consumers.

If the property changes buffer layout or shader metadata, include compilation,
reflection, uniform packing, and cache invalidation. If it changes bounds or shadow
behavior, include scene culling and geometry-instance dependencies.

## Add a scene pass

Start in the concrete method's orchestration. Specify required inputs, output
attachments, resolution, sample count, view count, load/store behavior, history, and
position relative to compositor callbacks.

Allocate resources through scene-buffer custom data or the appropriate owner. Create
compatible bindings and pipeline variants, then record RD operations. The device graph
can derive hazards from declared resources, but it cannot choose the semantic stage
placement for the method.

Review Mobile separately: a stage can break subpass merging or require an unsupported
buffer use. Reflection probes, XR, debug draws, transparent backgrounds, and scaled
outputs may take different paths. Compatibility needs an independent implementation or
an explicit capability limit.

## Change a device operation

Begin with [RenderingDeviceCommons](doc_rendering_classes_core) descriptions and the RD validation and resource
metadata. Follow the graph tracker/recorded operation, driver interface,
Vulkan/D3D12/Metal implementations, and relevant shader containers.

Preserve invalidation callbacks and deferred native retirement. Test local devices used
by baking/compression as well as the main renderer. A main-window draw test alone does
not exercise local submit/sync behavior.

## Remove an adapter

Write the current chain and label each representation, owner, and thread transition.
Identify the obligation that disappears or moves at every removed step. If no obligation
disappears, the change may be a useful relocation, but it is not yet evidence that the
overall design became smaller.

Start with private include cleanup or duplicate parameter conversion before merging
retained resources, device identities, and native lifetimes. The [wrapper
audit](doc_rendering_wrappers) provides concrete chains.

## Minimum regression scenarios

Choose cases affected by the contract: both RD methods, Compatibility where applicable,
multiple viewports/windows, threaded and single-thread rendering, creation/destruction
during loading, shader changes, resizing, local GPU tools, and XR/native imports when
their contracts change.

Record which platforms were executed and which received source review only. Do not infer
driver coverage from a headless run or from documentation tests.

Source routes: [server and scene execution](doc_rendering_source_map), [backend
implementations](doc_rendering_backend_sources), and [external integration
points](doc_rendering_integration_sources).
