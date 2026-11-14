# Vulkan driver

The Vulkan path implements RD through three cooperating layers: context, device driver,
and shader container. Platform subclasses supply native surface creation. The scene
renderer above RD contains no Vulkan-specific pass list.

## Context and surfaces

[RenderingContextDriverVulkan](doc_rendering_classes_drivers) manages Vulkan instance-level setup, physical device
information, surface support, debug/allocation support, and device driver construction.
It participates in the shared window-to-surface contract.

Platform subclasses include X11, Wayland, Windows, Android, macOS, and Apple embedded
variants. Their platform data describes the native window/display or layer needed for
surface creation. The display server calls context window operations as windows are
created, resized, or destroyed.

The Vulkan loader integration and `godot_vulkan.h` hide platform/API loading details
from much of the driver. Vulkan feature and extension checks remain inside the
context/device implementation; a directory named Vulkan does not guarantee that every
optional Vulkan feature is enabled.

## Device resources

[RenderingDeviceDriverVulkan](doc_rendering_classes_drivers) chooses features, creates the logical device, maps RD
formats and usage to Vulkan descriptions, and implements native buffers, images/views,
samplers, descriptor sets, shaders, pipelines, and render passes. Vulkan Memory
Allocator supplies allocation support.

The common driver IDs wrap identities that the Vulkan implementation resolves to its
records or API objects. RD continues to own the higher-level metadata, resource
dependencies, and frame retirement. Do not free a native image by bypassing its RD owner
while submitted frames may still use it.

## Commands and presentation

The driver implements command queues/pools/buffers, barriers and image-layout
transitions, graphics and compute commands, timestamps, fences, semaphores, and
swapchains. RD provides the command ordering and submission plan; Vulkan records and
executes the corresponding native operations.

Swapchain creation/recreation depends on the context's surface state, selected
formats/present modes, dimensions, and VSync. Presentation and rendering queue families
can differ. A minimized or resized window can temporarily have no drawable output; the
compositor handles failed screen preparation without treating every such case as a
renderer failure.

## Shader and cache path

[RenderingShaderContainerVulkan](doc_rendering_classes_drivers) packages SPIR-V stages and reflection-related data. It
can preserve debug code or use compressed representations. Native shader creation and
graphics/compute pipeline creation consume this data.

The Vulkan driver implements pipeline-cache persistence and derives a cache identity
from device/driver properties. This cache is separate from the renderer shader-bytecode
cache and live pipeline maps.

## Integration crossings

[VulkanHooks](doc_rendering_classes_drivers) allows an external subsystem such as OpenXR to participate in
instance/device and graphics binding setup. Hook registration and lifetime must precede
the corresponding creation operations.

The Vulkan implementation also conditionally includes Android OS/Java/thread
integration. That is a concrete driver-to-platform dependency, rather than platform code
depending only on a neutral driver interface.

On Apple platforms, a Vulkan route can use MoltenVK below Redot. That route still uses
these Vulkan context/device/container classes. Native Metal uses the separate [Metal
driver](doc_rendering_metal).

Sources: [Vulkan entry points](doc_rendering_backend_sources#vulkan), [platform
integration](doc_rendering_backend_sources#opengl-and-platforms), and [Android
crossing](doc_rendering_observation_sources#wrapper-and-ownership-observations).
