# Direct3D 12 driver

The D3D12 driver maps RD's resource, command, and synchronization contracts to Direct3D
12 and DXGI. It is shared by Forward+ and Mobile through RD; it does not supply another
scene rendering method.

## Context and device

[RenderingContextDriverD3D12](doc_rendering_classes_drivers) supplies adapter/device discovery, Windows surface data,
debug support, and device-driver construction. The Windows display server selects this
context and can try another compiled driver according to fallback settings.

[RenderingDeviceDriverD3D12](doc_rendering_classes_drivers) creates the device and checks required features, shader
capabilities, formats, and limits. It creates resources with D3D12 Memory Allocator, and
translates RD usage/binding descriptions to native resource and descriptor
representations.

The header's native helper types, allocation records, descriptors, command structures,
and state trackers are part of that translation. They are not additional scene APIs.
Their purpose includes adapting an RD contract that does not map one-to-one to D3D12
state.

## Resource binding and shaders

[RenderingShaderContainerD3D12](doc_rendering_classes_drivers) converts SPIR-V to NIR and DXIL when the NIR compilation
path is built. It generates resource-binding/root-signature data and retains
specialization metadata needed by pipeline creation. DXIL patching and signing helpers
also live with the shader-container implementation.

This path requires coordination among shader reflection, RD uniform sets, descriptor
tables, and native root parameters. Changing one set/binding convention can affect all
four representations.

Graphics and compute pipelines consume the generated shader data together with RD
pipeline state. A method's shader can consequently compile through the shared engine
language and GLSL stages before reaching D3D12-specific conversion and pipeline
creation.

## Command execution

The driver maps command queues, allocators/lists, barriers, copy operations, draws,
dispatches, fences, and swapchains to D3D12/DXGI. Queue execution calls
`ExecuteCommandLists`; presentation uses the DXGI swapchain's `Present()`.

RD manages frame-level resource retirement and the high-level dependency graph. The
driver translates the required access/resource state changes into native operations. RD
RIDs and D3D12 resource pointers must remain distinct when inspecting a transfer or
descriptor problem.

## Cache limitation

`pipeline_cache_create()` warns that PSO caching is not implemented and returns false.
The corresponding free/query/serialize methods report unimplemented operations. This
describes the native persistent pipeline-cache interface. It does not disable `ShaderRD`
bytecode caching or live RD pipeline reuse.

Implementing persistent PSO caching is a possible future project, but it needs cache
compatibility, device/driver invalidation, failure recovery, and Windows measurements.
It is not established as a small performance fix by the stub alone.

## External integration

[D3D12Hooks](doc_rendering_classes_drivers) supports graphics interoperation used by the OpenXR platform extension.
This puts device initialization and graphics binding under a cross-subsystem contract.
Engine changes must preserve that setup order even when no scene-level XR code appears
in the edited driver function.

ANGLE's Compatibility route is separate: it does not execute through this Redot D3D12
device driver.

Sources: [D3D12 entry points](doc_rendering_backend_sources#direct3d-12), [Windows
selection](doc_rendering_backend_sources#opengl-and-platforms), and [cache
limitation](doc_rendering_observation_sources#incomplete-and-disabled-paths).
