# Native Metal driver

The native Metal route implements RD with Objective-C++ driver code, Metal
resource/command wrappers, shader conversion, and Apple surface integration. It is
distinct from the Vulkan route through MoltenVK.

## Context and capabilities

[RenderingContextDriverMetal](doc_rendering_classes_drivers) obtains a Metal device and exposes its capability record
through the context interface. In this snapshot it reports one device. Its surface
implementation wraps a `CAMetalLayer`, manages drawables, applies size/VSync state, and
supplies framebuffer records for presentation.

[MetalDeviceProperties](doc_rendering_classes_drivers), [MetalFeatures](doc_rendering_classes_drivers), [MetalLimits](doc_rendering_classes_drivers), and pixel-format helpers
translate device-family and OS capabilities into RD limits and format support.
Build/availability annotations in the headers are not a guarantee that all features
exist on every supported Apple target.

## Resource and command wrappers

[RenderingDeviceDriverMetal](doc_rendering_classes_drivers) creates buffers, textures/views, samplers, bindings,
graphics/compute pipelines, command structures, and synchronization objects. Resource
options include tracked hazards; buffers/textures use storage modes appropriate to their
CPU/GPU and attachment uses.

`metal_objects.h` / `.mm` hold objects such as [MDCommandBuffer](doc_rendering_classes_drivers), pipeline wrappers,
framebuffer representations, and bound-uniform data. `MDCommandBuffer` coordinates Metal
encoders and state. This layer adapts RD's command contract to Metal's encoder model; it
is not a second scene renderer.

## Shaders

[RenderingShaderContainerMetal](doc_rendering_classes_drivers) uses SPIRV-Cross and device/target profiles to prepare
Metal shader code and reflection/binding data. Native pipeline creation then uses that
representation. Uniform bindings, argument buffers, immutable samplers, specialization
choices, and Metal feature limits must agree.

The conversion path shares engine shader parsing and RD templates with other RD drivers,
but not their final shader containers or native pipeline objects.

## Native integration above RD

MetalFX lives in the RD renderer's effect directory and uses native Metal interfaces. It
obtains driver resources and adds an RD driver callback with declared texture usages.
The callback encodes MetalFX work into the native command buffer and retains the scaler
through a completion handler.

This is a verified crossing from effect code into concrete driver types. The source TODO
asks for a general API to retain objects until command-buffer completion. A refactor
must preserve both graph resource declarations and Objective-C object lifetime.

## Incomplete paths

The native pipeline-cache creation method returns false before binary-archive setup.
Secondary command-buffer methods reject requests as unimplemented; the common RD
configuration currently requests zero secondary buffers. The explicit driver barrier
method warns once and does no barrier work.

These facts do not by themselves prove that normal Metal rendering lacks
synchronization: resources request Metal hazard tracking and encoder behavior differs
from Vulkan. A proposal for explicit hazards needs analysis of those native contracts
and cross-encoder tests.

Other markers cover intermediate-buffer texture copies, tessellation, buffer-texture
binding forms, breadcrumbs, and memory accounting. Consult the [marker
catalog](doc_rendering_reference) and [unfinished-path
assessment](doc_rendering_unfinished) before treating a marker as reachable or as a
small implementation task.

Sources: [Metal entry points](doc_rendering_backend_sources#metal), [MetalFX
callback](doc_rendering_source_map#shaders-and-effects), and [cache/barrier
stubs](doc_rendering_observation_sources#incomplete-and-disabled-paths).
