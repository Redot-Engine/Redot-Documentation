# Textures and transfers

A scene texture, its server record, its RD image/view, and its native GPU allocation can
have different identities and lifetimes. This page follows those transitions and the
operations that can force synchronization.

## Create a scene texture

[ImageTexture](class_ImageTexture) exposes scene resource behavior and delegates GPU work to
[RenderingServer](class_RenderingServer). Texture creation allocates a server RID and initializes the selected
texture storage. The RD implementation validates/converts the image format, creates an
RD texture, and can create a matching sRGB view. GLES3 storage creates GL textures
through its own path.

The server texture record retains dimensions, formats, proxy relationships, and renderer
metadata. Removing this record in favor of a bare RD RID would change more than
allocation: canvas, materials, import/readback paths, and non-RD implementations rely on
the server texture contract.

## Upload and download

RD represents texture formats, usage flags, views, slices, and transfer operations
independently of the native API. Its upload paths use staging memory and transfer
workers. Submission integrates their work with graphics commands through barriers and
semaphores.

Staging buffers have size and reuse limits. Under pressure, the allocator can flush and
stall frames. The source mentions a possible future alternative that blocks a loading
thread until more staging room becomes available; that alternative is not implemented in
this snapshot.

Readback must move data from GPU storage to CPU-visible memory and observe completion.
Synchronous texture/image retrieval in a frequent update loop can therefore introduce
waits that a setter-only workload does not have. Check which readback path the caller
uses before changing its synchronization.

## Bridge an existing RD texture

`Texture2DRD::set_texture_rd_rid()` schedules its update on the render thread. The
implementation validates the RID against the main RD, obtains its format, and creates or
replaces the server-side texture wrapper.

`TextureStorage::texture_rd_initialize()` requires a sampling-capable texture and
rejects an already-shared RD texture. It creates shared views of the original image so
that the wrapper does not take ownership of that original. This restriction and the
source's slice TODOs matter when designing a broader texture-view API.

The layered and 3D bridge classes follow related contracts. They do not provide a
mechanism for sharing a local RD's resources with the main RD: RID ownership is
device-specific.

## Proxies and native imports

Texture proxies redirect scene-level texture references. Replacing a texture can
preserve the server identity used by existing consumers while changing the backing
representation. Proxy cleanup must detach relationships and invalidate dependent state.

Native texture import enters through a separate API. A native handle carries
backend-specific ownership, format, and synchronization requirements. XR and camera
integrations can supply images created outside the normal allocator. The common integer
parameter does not establish a shared ownership model across Vulkan, GL, D3D12, and
Metal.

## Render targets

Texture storage also owns render targets. These include output images, attachments,
readback/copy state, and flags such as whether another viewport used the target. This
connects texture storage to viewport scheduling and scene-buffer format selection. See
[viewports](doc_rendering_viewports) for those consumers and
[ownership](doc_rendering_ownership) for deferred deletion.

Sources: [texture/device implementation](doc_rendering_source_map#resources-and-device),
[scene texture bridges](doc_rendering_integration_sources#scene-resources), and [staging
observations](doc_rendering_observation_sources#performance-investigation-points).
