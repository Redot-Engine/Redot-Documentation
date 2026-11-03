# Mermaid diagrams

Store diagram source in separate `.mmd` files next to the documentation that uses
them. A `diagrams/` subfolder keeps large definitions out of Markdown articles and
out of the navigation menu.

## Create a diagram

For an article at `docs/26.3/Tutorials/example.md`, create
`docs/26.3/Tutorials/diagrams/scene-lifecycle.mmd`. The file contains Mermaid
source without Markdown fences:

```text
flowchart LR
    accTitle: Scene lifecycle
    accDescr: A scene starts, runs, and then exits.
    Start --> Running --> Exit
```

The site uses [Mermaider](https://github.com/nullean/mermaider) 0.15.1 to render
SVGs in C# at startup. Its syntax support and layout can differ from Mermaid.js
and the Mermaid Live Editor. Test the result on the documentation site before
submitting it. Describe complex diagrams in the surrounding article as well.

## Embed a diagram

Put an embed on its own line, with blank lines before and after it. Both paired
and self-closing tags work:

```html
<mermaid ref="./diagrams/scene-lifecycle.mmd"></mermaid>

<mermaid ref="./diagrams/scene-lifecycle.mmd" />
```

Use `slug` to reference a diagram by filename:

```html
<mermaid slug="scene-lifecycle"></mermaid>

<mermaid slug="scene-lifecycle" />
```

Provide exactly one of `ref` or `slug`. Attribute values must be quoted; single
and double quotes work. Paired tags must have an empty body. Embeds work inside
tabs and callouts. Tags inside code examples and HTML comments remain literal.

The optional `alt` attribute supplies alternative text for the generated image;
otherwise the site uses the diagram's `accTitle`, or its filename when no title
is supplied. Use `caption` for a visible caption. Both are plain text:

```html
<mermaid slug="scene-lifecycle"
         alt="A scene starts, runs, and then exits."
         caption="The three stages of the scene lifecycle." />
```

Escape special characters in attributes with HTML entities, such as `&amp;`
and `&quot;`. Captions appear in documentation search; diagram source and
rendering errors do not.

## Paths and slugs

Relative paths resolve from the article's directory. Parent segments such as
`../diagrams/example.mmd` work when they stay within the same documentation
scope. A leading slash starts at the selected version's root:

```html
<mermaid ref="/Tutorials/diagrams/scene-lifecycle.mmd" />
```

Paths must name local `.mmd` files. Use literal spaces and Unicode characters,
not URL encoding. External URLs, query strings, fragments, and symbolic links
are not supported. Path matching ignores case, but two paths that differ only
by case are an error.

Slugs are derived from the filename without `.mmd`: letters become lowercase,
and spaces, underscores, and punctuation become hyphens. For example,
`Scene_Lifecycle.mmd` becomes `scene-lifecycle`. Slugs are case-insensitive.
If two files in one scope produce the same slug, use `ref` to select one.

## Versioned and shared diagrams

Keep release-specific diagrams inside their release directory. An embed in
`26.3` resolves slugs in `26.3`; the same embed in `latest` resolves them in
`latest`. Missing diagrams do not fall back to another release. Unversioned
documentation URLs use the latest stable version configured in `Versions.json`.

Shared diagrams live under `docs/About/`, `docs/Community/`, or
`docs/Contributing/`. Shared articles resolve slugs in that shared scope,
independently of the selected release. A versioned article can reference a
shared diagram explicitly:

```html
<mermaid ref="/Contributing/Documentation/diagrams/contribution-flow.mmd" />
```

When creating a release directory, include its diagram files with its Markdown
files. Moving a diagram changes its relative paths but preserves its slug if
the filename stays the same. Renaming it changes its slug; update all affected
embeds. Generated SVG URLs are cache artifacts and should not be used directly
in articles.

## Preview and validation

Run the documentation project with `dotnet run --project Redot-Documentation`.
Restart after editing `.mmd` files: diagram discovery and rendering happen at
startup. Cached SVGs live under `Redot-Documentation/App_Data/mermaid/` and are
reused when source, renderer version, and rendering settings match. This
directory is generated, excluded from Git, and may be removed to rebuild it.
The site can still render diagrams when the persistent cache is not writable.

Run `dotnet test` before submitting changes. The documentation audit checks
all configured versions, renders every diagram, and verifies embeds. A bad
reference or diagram displays an error in its article; rendering failures are
also recorded in the server log. Other diagrams and articles remain available.
Sources are limited to 512 KiB; Mermaider also limits rendering time and graph
size. Split diagrams that exceed these limits into smaller explanations.
