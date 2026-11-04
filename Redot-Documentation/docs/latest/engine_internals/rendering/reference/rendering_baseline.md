# Rendering source baseline

This manual describes local engine master at Redot 26.4.0 alpha. The narrative review
date is 2026-11-04. Source references share this snapshot so that quarterly refreshes
can update one baseline and regenerate the reference material.

- Source revision: [82c6c949656f](https://github.com/Redot-Engine/redot-engine/commit/82c6c949656f8af31a37e0381048beb381fcbb27).
- Narrative review revision: `82c6c949656f8af31a37e0381048beb381fcbb27`.
- Primary source coverage: 140 headers and 235 other source files.
- Marker catalog: 180 matching source lines.
- External search index: 291 files.

The native backends are described from source inspection. Documentation tests validate
links, routing, and Mermaid rendering; they do not validate GPU behavior on Windows,
Apple, mobile, or web hardware. GDExtension and redot-cpp are outside this manual's
scope.

The primary coverage includes every committed header, implementation, and shader in
servers/rendering, GLES3, Vulkan, D3D12, Metal, and EGL, plus the GL context build
directory. Platform, module, scene, and core integration points appear in the source
maps and dependency pages. Generated files and third-party library implementations are
excluded.
