#!/usr/bin/env python3
"""Refresh and verify the rendering manual's source-derived reference pages.

Uses committed Git objects, never generated files or uncommitted engine changes.
No dependencies outside Python's standard library and Git.
"""

import argparse
import ast
import datetime
import hashlib
import json
from pathlib import Path, PurePosixPath
import posixpath
import re
import subprocess
import sys
import textwrap
from urllib.parse import quote


REPO = Path(__file__).resolve().parents[1]
DOCS = REPO / "Redot-Documentation/docs/latest/engine_internals/rendering"
REFERENCE = DOCS / "reference"
SNAPSHOT = DOCS / "rendering_snapshot.json"
ANCHORS = DOCS / "rendering_source_anchors.json"
ROOTS = (
    "servers/rendering/", "drivers/gles3/", "drivers/vulkan/",
    "drivers/d3d12/", "drivers/metal/", "drivers/egl/", "drivers/gl_context/",
)
CODE_SUFFIXES = {".h", ".cpp", ".mm", ".c", ".glsl", ".glslinc", ".metal", ".inc"}
COMMENT_MARKER = re.compile(r"\b(?:TODO|FIXME|HACK)\b", re.I)
DIAGNOSTIC_MARKER = re.compile(r"not (?:yet )?implemented|unimplemented", re.I)
MARKED_DIAGNOSTIC = re.compile(
    r'\b(?:DEV_ASSERT|ERR_[A-Z_]+)\s*\([^;\n]*?"[^"\n]*\b(?:TODO|FIXME|HACK)\b'
)
# Consume literals before looking inside them for comment delimiters. This is a
# lexical C/C++/shader scan, including raw strings and escaped quote characters.
SOURCE_TOKEN = re.compile(
    r'(?P<comment>//(?:\\\r?\n|[^\n])*|/\*[\s\S]*?(?:\*/|\Z))'
    r'|(?:u8|u|U|L)?R"(?P<delimiter>[^ ()\\\t\r\n]{0,16})\([\s\S]*?\)(?P=delimiter)"'
    r'|"(?:\\[\s\S]|[^"\\])*"'
    r"|(?<![\w'])'(?:\\[^\n]|[^'\\\n])*'"
)
EXTERNAL = re.compile(
    r'#\s*(?:include|import)\s*[<"](?:servers/rendering/|drivers/(?:gles3|vulkan|d3d12|metal)/)'
    r"|\b(?:RenderingServer|RenderingDevice|RS|RD|RSG)::"
    r"|\b(?:ShaderLanguage|ShaderCompiler|RendererCompositor|RenderSceneBuffersRD|VulkanHooks|D3D12Hooks)\b"
)
GROUPS = (
    ("core", "Core renderer classes", ("servers/rendering/",)),
    ("storage", "Shared storage classes", ("servers/rendering/storage/", "servers/rendering/environment/")),
    ("rd", "RenderingDevice renderer classes", ("servers/rendering/renderer_rd/",)),
    ("rd_storage", "RenderingDevice storage classes", ("servers/rendering/renderer_rd/storage_rd/",)),
    ("methods", "Forward renderer classes", ("servers/rendering/renderer_rd/forward_clustered/", "servers/rendering/renderer_rd/forward_mobile/")),
    ("effects", "Environment and effect classes", ("servers/rendering/renderer_rd/effects/", "servers/rendering/renderer_rd/environment/")),
    ("gles", "Compatibility renderer classes", ("drivers/gles3/",)),
    ("drivers", "Native driver classes", ("drivers/vulkan/", "drivers/d3d12/", "drivers/metal/", "drivers/egl/", "drivers/gl_context/")),
    ("dummy", "Dummy renderer classes", ("servers/rendering/dummy/",)),
)


def git(engine, *args):
    result = subprocess.run(["git", "-C", str(engine), *args], capture_output=True)
    if result.returncode:
        raise ValueError(result.stderr.decode(errors="replace").strip())
    return result.stdout


def group_for(path):
    matches = [(len(prefix), key) for key, _, prefixes in GROUPS
               for prefix in prefixes if path.startswith(prefix)]
    return max(matches)[1]


def wrap(text):
    return "\n\n".join(
        textwrap.fill(p, width=88, break_long_words=False, break_on_hyphens=False)
        for p in text.split("\n\n")
    )


def source_link(snapshot, path, line=1):
    return f"{snapshot['repository']}/blob/{snapshot['source_revision']}/{quote(path, safe='/')}#L{line}"


def source_markers(content):
    """Yield marked source lines, excluding TODO-like identifiers and literals."""
    comment_lines = set()
    line_number = 1
    previous_end = 0
    for token in SOURCE_TOKEN.finditer(content):
        line_number += content.count("\n", previous_end, token.start())
        text = token.group()
        if token.group("comment") is not None:
            comment_lines.update(line_number + offset for offset, line in enumerate(text.splitlines())
                                 if COMMENT_MARKER.search(line))
        line_number += text.count("\n")
        previous_end = token.end()
    for number, line in enumerate(content.splitlines(), 1):
        # Explicit diagnostics such as ERR_FAIL_MSG("Not implemented") remain
        # useful stub evidence even though they are string literals.
        if (number in comment_lines or DIAGNOSTIC_MARKER.search(line)
                or MARKED_DIAGNOSTIC.search(line)):
            yield {"line": number, "text": line.strip()}


def locate_source(content, path, locator):
    if not locator:
        raise ValueError(f"Empty source locator: {path}")
    matches = [n for n, line in enumerate(content.splitlines(), 1) if locator in line]
    if not matches:
        raise ValueError(f"Missing source locator: {path}: {locator}")
    if len(matches) != 1:
        raise ValueError(f"Ambiguous source locator: {path}: {locator}")
    return matches[0]


def build(engine, snapshot):
    revision = snapshot["source_revision"]
    paths = git(engine, "ls-tree", "-r", "--name-only", revision).decode().splitlines()
    path_set = set(paths)
    primary = [p for p in paths if p.startswith(ROOTS) and PurePosixPath(p).suffix in CODE_SUFFIXES]
    cache = {}

    def read(path):
        if path not in cache:
            cache[path] = git(engine, "show", f"{revision}:{path}").decode(errors="replace")
        return cache[path]

    # Search outside the renderer too. Git grep excludes ignored build outputs and
    # returns evidence at the same revision used by the reference links.
    grep = subprocess.run(
        ["git", "-C", str(engine), "grep", "-n", "-I", "-E",
         r'#\s*(include|import).*(servers/rendering/|drivers/(gles3|vulkan|d3d12|metal)/)|'
         r'\b(RenderingServer|RenderingDevice|RS|RD|RSG)::|'
         r'\b(ShaderLanguage|ShaderCompiler|RendererCompositor|RenderSceneBuffersRD|VulkanHooks|D3D12Hooks)\b',
         revision, "--", "core", "scene", "editor", "modules", "platform", "main", "servers", "tests"],
        capture_output=True, text=True,
    )
    if grep.returncode not in (0, 1):
        raise ValueError(grep.stderr)
    external = {}
    for result in grep.stdout.splitlines():
        _, path, number, evidence = result.split(":", 3)
        if path.startswith(ROOTS) or PurePosixPath(path).suffix not in {".h", ".cpp", ".mm"}:
            continue
        if EXTERNAL.search(evidence):
            external.setdefault(path, []).append({"line": int(number), "text": evidence.strip()})

    inventory = []
    todos = []
    for path in primary:
        content = read(path)
        declarations = []
        includes = []
        # This deliberately remains a lexical index. It includes nested helper
        # classes and structs, not forward declarations, and does not claim C++ parsing.
        for number, line in enumerate(content.splitlines(), 1):
            stripped = line.strip()
            include = re.match(r'\s*#\s*(?:include|import)\s*["<]([^">]+)[">]', line)
            if include:
                target = include[1]
                local = posixpath.normpath(str(PurePosixPath(path).parent / target))
                resolved = target if target in path_set else local if local in path_set else None
                includes.append({"line": number, "written": target, "resolved": resolved})
            if re.match(r"(?:class|struct)\s+", stripped) and not re.match(r"(?:class|struct)\s+\w+\s*;", stripped):
                found = re.match(r"(?:class|struct)\s+(?:API_AVAILABLE\(.*?\)\s+)?(\w+)", stripped)
                if found:
                    declarations.append({"name": found[1], "line": number})
        todos.extend({"path": path, **marker} for marker in source_markers(content))
        inventory.append({
            "path": path, "sha256": hashlib.sha256(content.encode()).hexdigest(),
            "lines": len(content.splitlines()), "group": group_for(path),
            "declarations": declarations if path.endswith(".h") else [],
            "includes": includes,
        })

    outputs = {}
    preface = wrap(
        "This index comes from the [rendering baseline](doc_rendering_baseline). "
        "Links identify committed source. Declarations include nested helper types; "
        "they do not imply separate runtime components. Conditional compilation "
        "and generated classes require the [build map](doc_rendering_build)."
    )
    for key, title, _ in GROUPS:
        headers = [i for i in inventory if i["group"] == key and i["path"].endswith(".h")]
        rows = [f"# {title}", "", preface, "", "| Header and implementation | Declared types |", "|---|---|"]
        for item in headers:
            path = item["path"]
            links = f"[{path}]({source_link(snapshot, path)})"
            for suffix in (".cpp", ".mm"):
                companion = str(PurePosixPath(path).with_suffix(suffix))
                if companion in paths:
                    links += f" · [{suffix[1:]}]({source_link(snapshot, companion)})"
            types = ", ".join(f"[{d['name']}]({source_link(snapshot, path, d['line'])})" for d in item["declarations"]) or "Definitions, aliases, or API includes."
            rows.append(f"| {links} | {types} |")
        outputs[f"rendering_classes_{key}.md"] = "\n".join(rows) + "\n"

    # Keep raw markers and external search evidence in JSON. Human-facing pages
    # page the results so that they remain navigable and independently reviewable.
    for offset in range(0, len(todos), 24):
        part = offset // 24 + 1
        rows = [f"# Renderer source markers {part}", "", wrap(
            "These are source markers, not confirmed defects or approved work. "
            "Read the containing function and its callers before classifying a marker. "
            "Some describe intentional API limits or old comments. See "
            "[unfinished paths](doc_rendering_unfinished) for assessed examples. "
            "The [maintenance procedure](doc_rendering_maintenance) rebuilds this catalog."
        ), "", "| Source | Marker text |", "|---|---|"]
        for entry in todos[offset:offset + 24]:
            text = entry["text"].replace("|", "&#124;").replace("<", "&lt;").replace(">", "&gt;").replace("`", "'")
            rows.append(f"| [{entry['path']}:{entry['line']}]({source_link(snapshot, entry['path'], entry['line'])}) | {text} |")
        outputs[f"rendering_markers_{part:02}.md"] = "\n".join(rows) + "\n"

    for area in ("core", "scene", "editor", "modules", "platform", "main", "servers", "tests"):
        selected = [(p, entries) for p, entries in sorted(external.items()) if p.startswith(area + "/")]
        for offset in range(0, len(selected), 35):
            part = offset // 35 + 1
            rows = [f"# Renderer consumers in {area} {part}", "", wrap(
                "This search index lists direct renderer includes and selected renderer "
                "symbol references. It includes conditional and test code. It is not a "
                "complete transitive dependency graph. The manually assessed "
                "[dependency map](doc_rendering_dependencies) explains the contracts. "
                "Follow each link to inspect the first matching source line."
            ), "", "| Source file | Matching lines |", "|---|---|"]
            for path, entries in selected[offset:offset + 35]:
                numbers = ", ".join(str(e["line"]) for e in entries)
                rows.append(f"| [{path}]({source_link(snapshot, path, entries[0]['line'])}) | {numbers} |")
            outputs[f"rendering_consumers_{area}_{part:02}.md"] = "\n".join(rows) + "\n"

    for page in json.loads(ANCHORS.read_text()):
        rows = [f"# {page['title']}", "", wrap(
            "These source entry points support the architecture pages. Their revision "
            "and review date appear once in the [baseline](doc_rendering_baseline). "
            "The refresh tool relocates symbols when line numbers change."
        )]
        for section in page["sections"]:
            rows.extend(["", f"## {section['title']}", ""])
            for entry in section["entries"]:
                content = read(entry["path"])
                locator = entry.get("find", "")
                number = locate_source(content, entry["path"], locator)
                rows.append(f"- [{entry['label']}]({source_link(snapshot, entry['path'], number)})")
        outputs[page["file"]] = "\n".join(rows) + "\n"

    header_count = sum(i["path"].endswith(".h") for i in inventory)
    baseline = ["# Rendering source baseline", "", wrap(
        f"This manual describes local engine master at Redot {snapshot['engine_version']}. "
        f"The narrative review date is {snapshot['reviewed_on']}. "
        "Source references share this snapshot so that quarterly refreshes can update "
        "one baseline and regenerate the reference material."
    ), "", f"- Source revision: [{revision[:12]}]({snapshot['repository']}/commit/{revision}).",
        f"- Narrative review revision: `{snapshot['reviewed_revision']}`.",
        f"- Primary source coverage: {header_count} headers and {len(inventory) - header_count} other source files.",
        f"- Marker catalog: {len(todos)} matching source lines.",
        f"- External search index: {len(external)} files.", "", wrap(
            "The native backends are described from source inspection. Documentation "
            "tests validate links, routing, and Mermaid rendering; they do not validate "
            "GPU behavior on Windows, Apple, mobile, or web hardware. GDExtension and "
            "redot-cpp are outside this manual's scope."
        ), "", wrap(
            "The primary coverage includes every committed header, implementation, "
            "and shader in servers/rendering, GLES3, Vulkan, D3D12, Metal, and EGL, "
            "plus the GL context build directory. Platform, module, scene, and core "
            "integration points appear in the source maps and dependency pages. "
            "Generated files and third-party library implementations are excluded."
        )]
    if snapshot["reviewed_revision"] != revision:
        baseline += ["", ":::warning", "", "The source indexes are newer than the narrative review. Review changed contracts before treating the prose as current.", "", ":::"]
    outputs["rendering_baseline.md"] = "\n".join(baseline) + "\n"
    data = {"source_revision": revision, "primary": inventory, "external": external, "markers": todos}
    outputs["rendering_inventory.json"] = json.dumps(data, indent=2) + "\n"
    references = ["# Rendering source reference", "", wrap(
        "Use the class indexes to locate a header and its implementation. Use the "
        "source maps for important execution paths. The consumer and marker indexes "
        "are reproducible investigation aids; the architecture pages explain which "
        "relationships matter."
    ), ""]
    for name, content in outputs.items():
        if name.endswith(".md"):
            title = content.splitlines()[0][2:]
            references.append(f"- [{title}](doc_{PurePosixPath(name).stem})")
    references += ["", wrap(
        "The adjacent rendering_inventory.json retains hashes, declarations, direct "
        "include/import edges, marker text, and search evidence for update tools. "
        "Include edges are syntactic: conditional branches are not evaluated. "
        "Unresolved edges can refer to system, generated, or external-library headers."
    )]
    outputs["index.md"] = "\n".join(references) + "\n"
    return outputs


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("action", choices=("refresh", "check", "review"))
    parser.add_argument("--engine", required=True, type=Path)
    parser.add_argument("--revision", default="master", help="Refresh from this committed engine revision")
    parser.add_argument("--date", help="Review date, YYYY-MM-DD; required for review")
    args = parser.parse_args()
    snapshot = json.loads(SNAPSHOT.read_text())
    if args.action == "refresh":
        previous = snapshot["source_revision"]
        snapshot["source_revision"] = git(args.engine, "rev-parse", "--verify", "--end-of-options", args.revision + "^{commit}").decode().strip()
        namespace = {}
        version_tree = ast.parse(git(args.engine, "show", f"{snapshot['source_revision']}:version.py"))
        for node in version_tree.body:
            if isinstance(node, ast.Assign) and len(node.targets) == 1 and isinstance(node.targets[0], ast.Name):
                namespace[node.targets[0].id] = ast.literal_eval(node.value)
        snapshot["engine_version"] = f"{namespace['major']}.{namespace['minor']}.{namespace['patch']} {namespace['status']}"
        if previous != snapshot["source_revision"]:
            changed = git(args.engine, "diff", "--stat", previous, snapshot["source_revision"], "--", *ROOTS).decode()
            print("Renderer changes since previous snapshot:\n" + changed)
            print("Narrative review remains at " + snapshot["reviewed_revision"])
    elif args.action == "review":
        if not args.date:
            parser.error("review requires --date YYYY-MM-DD after manual narrative review")
        datetime.date.fromisoformat(args.date)
        snapshot["reviewed_revision"] = snapshot["source_revision"]
        snapshot["reviewed_on"] = args.date

    outputs = build(args.engine, snapshot)
    expected = {REFERENCE / name: text for name, text in outputs.items()}
    expected[SNAPSHOT] = json.dumps(snapshot, indent=2) + "\n"
    stale = [p for p, text in expected.items() if not p.exists() or p.read_text() != text]
    obsolete = [p for p in REFERENCE.glob("rendering_*") if p not in expected]
    if args.action == "check":
        if stale or obsolete:
            for path in stale + obsolete:
                print("Stale generated reference: " + str(path.relative_to(REPO)), file=sys.stderr)
            return 1
        print(f"Verified {len(outputs)} generated reference files at {snapshot['source_revision'][:12]}.")
    else:
        REFERENCE.mkdir(parents=True, exist_ok=True)
        for path in obsolete:
            path.unlink()
        for path in stale:
            path.write_text(expected[path])
        print(f"Updated {len(stale)} files; source locators and committed paths verified.")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (ValueError, OSError, KeyError) as error:
        print(str(error), file=sys.stderr)
        sys.exit(1)
