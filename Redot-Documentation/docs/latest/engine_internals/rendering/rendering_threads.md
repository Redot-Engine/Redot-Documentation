# Rendering threads and synchronization

Rendering has several kinds of concurrency: scene changes from the main thread, server
command dispatch, CPU worker tasks, asynchronous resource work, and GPU queues. A
function being thread-safe does not mean its result is ready for GPU use or available to
the caller immediately.

## Server dispatch

[RenderingServerDefault](doc_rendering_classes_core) contains `CommandQueueMT`, a server thread identity, and a
worker-pool pump task. Its dispatch macros compare the caller's identity with the server
thread. Ordinary setters can queue work and return. Methods that need a result use
return/synchronization variants of the macros; these can block the caller. Even a direct
call can flush pending commands first.

`server_wrap_mt_common.h` supplies the macro bodies. The surrounding block in
`rendering_server_default.h` supplies `ServerName` and `server_name`, which select the
destination interface and `RSG` pointer. To understand one method, read both the macro
invocation and that block's destination definitions.

Texture creation has specialized wrappers. They allocate a server RID first, then
initialize its storage record directly when already on the server thread or when the
compositor permits asynchronous resource creation. Otherwise, initialization is queued.
The RD compositor reports this permission as true; GLES3 and dummy report false. This
exception does not make every storage or device method callable from arbitrary threads.

<mermaid ref="./diagrams/rendering-threads.mmd"
         alt="Main thread calls either execute on the server thread or enter its command queue; CPU workers and GPU queues have separate completion points." />

## CPU synchronization

With a separate rendering thread, the pump yields through `WorkerThreadPool` and flushes
queued commands. `RenderingServerDefault::sync()` synchronizes that queue. In
single-thread mode it flushes commands pending from other threads. This operation
establishes server progress, not a universal GPU-idle point.

Scene culling uses worker-pool group tasks above its culling threshold. Pipeline and
shader preparation can also use worker tasks. Those tasks have their own completion and
lifetime requirements. Finalization must not delete objects still referenced by a
compilation task.

`ResourceLoader::_ensure_load_progress()` exposes a cross-subsystem dependency: in
single-thread rendering it calls server `sync()` so resource loading can progress.
Removing a queue or changing who pumps it must preserve this loading path. The source
explicitly identifies the coupling as future refactoring work.

## Device and GPU synchronization

RD records a render-thread identity. Command-list and many resource-query operations
enforce that identity through `ERR_RENDER_THREAD_GUARD`. Other creation/transfer
operations have separate concurrency mechanisms; inspect each operation rather than
assuming one rule for all of RD.

The main RD belongs to the engine's frame loop. A local RD has its own command recording
and uses `submit()` followed by `sync()` to wait for its submitted work. Main-device
calls to those local submission methods are rejected. A RID from one device cannot be
treated as a resource of another device.

Frame slots use GPU fences before reuse. Transfers use staging memory, transfer workers,
barriers, and semaphores. A CPU queue flush cannot replace those resource-hazard and
GPU-completion mechanisms.

## Refactoring checks

For a proposed direct call, identify the required thread, whether arguments must survive
queuing, whether the result forces a wait, and whether GPU work can still reference the
resource. The wrapper may be carrying any of those contracts even when its body appears
to forward a single function call.

Sources: [dispatch macros and pump](doc_rendering_source_map#startup-and-dispatch),
[device contracts](doc_rendering_source_map#resources-and-device), and [resource
loading](doc_rendering_integration_sources#services-and-modules).
