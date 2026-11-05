# Resource ownership and handles

Rendering crosses several ownership systems. Keep their identities separate when
following a texture, mesh, material, or instance through the engine.

| Identity | Owner and lifetime |
|---|---|
| [Object](class_Object) / [Resource](class_Resource) | Scene/core object lifetime; resources commonly use `Ref` reference counting |
| RenderingServer RID | A renderer/server owner's record, explicitly released through the server API |
| RenderingDevice RID | An RD owner's validated graphics-resource record, explicitly released on that device |
| Driver ID | Typed [RenderingDeviceDriver](doc_rendering_classes_core) identity for a backend resource |
| Native handle | API-specific Vulkan, D3D12, Metal, or GL object; ownership depends on how it was created/imported |

The same `RID` C++ type represents different domains. Its numeric value is not a
portable texture pointer or a reference-counted resource. A texture RID returned by
`Texture2D::get_rid()` is not interchangeable with the RD texture RID returned by a
texture bridge.

## Scene and storage ownership

[VisualInstance3D](class_VisualInstance3D) constructs a server instance and releases it in its destructor. The
instance references a base resource such as a mesh. A single mesh can be referenced by
many instances, so removing one instance does not mean its mesh must be deleted.

Storage implementations use `RID_Owner` to allocate and validate resource records. Some
operations split allocation from initialization to support queued creation. The owner
must know the RID before initialization runs; code that looks up the record must respect
that transition.

`RenderingServerDefault::_free()` tries resource utilities, canvas, viewport, and scene
owners. The first owner that recognizes the RID handles the request. The dispatch route
is part of the server API; a caller must not pick an owner by guessing the RID's
contents.

## Dependencies and deletion

There are two distinct dependency systems:

- Renderer [Dependency](doc_rendering_classes_storage) / [DependencyTracker](doc_rendering_classes_storage) notifications invalidate
  scene/geometry state when resources change or disappear.
- RD dependencies connect GPU resources, such as textures, uniform sets,
  framebuffers, shaders, and pipelines. They support validity checks,
  invalidation callbacks, and cascading release of dependent objects.

Neither system is a replacement for `Ref` ownership. A dependency notification can tell
an instance to rebuild cached state without transferring ownership of the referenced
material or texture.

## GPU retirement

RD's `_free_internal()` removes the RID from its owner and queues native resources for
disposal in a frame slot. `_begin_frame()` waits before reusing that slot and frees
pending resources. A CPU-visible RID can become invalid before the native object is
destroyed.

This is why replacing RD deletion with an immediate driver `free` call can introduce
use-after-free on the GPU even if CPU tests pass. Conversely, retaining a scene resource
does not guarantee that an independently managed RD RID remains valid.

<mermaid ref="./diagrams/rendering-lifetimes.mmd"
         alt="Scene resource lifetime, server records, device records, and deferred native resource retirement are distinct stages." />

## Borrowing and views

[Texture2DRD](class_Texture2DRD) bridges an existing texture of the main RD into the scene texture API. RD
texture storage creates shared views so that the server wrapper does not acquire
ownership of the original allocation. Freeing the wrapper and freeing the original
texture are different operations.

Native imports, XR images, texture proxies, shared slices, and render targets have their
own constraints. Record the owner of the underlying allocation, which views borrow it,
and which caches must be invalidated before changing these paths. See [texture
transfers](doc_rendering_textures).

Sources: [resource deletion and
dependencies](doc_rendering_source_map#resources-and-device) and [scene resource
bridges](doc_rendering_integration_sources#scene-resources).
