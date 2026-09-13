# Redot Engine documentation

This repository contains the source files of [Redot Engine](https://redotengine.org)'s documentation, in Markdown.

This is a rewrite of our documentation website, and is a transition from Sphinx over to a custom solution in ASP.NET Blazor.

The site is still under heavy construction and is not ready yet, but we hope to have it online before the release of Redot 26.2-stable.

## Development

### IDE
There are no detailed build instructions at this time. The use of an IDE such as Visual Studio or JetBrains Rider is highly recommended. Simply build the solution and run the `Redot-Documentation` project.
The docs themselves can be found in the `Redot-Documentation/docs/` folder, and are just standard markdown files.

---

### CLI
For running from the CLI, you can build and run with the following commands:
```bash
dotnet restore
dotnet build
dotnet run --project Redot-Documentation/Redot-Documentation.csproj
```
Note that after you have built the project at least once, you can skip the `dotnet restore` and `dotnet build` steps.

### Documentation versions

Documentation versions are configured in `Redot-Documentation/docs/Versions.json`. Each entry contains:

- `Slug`: Stable URL and local directory identifier, such as `26.1`.
- `FriendlyName`: Label shown in the version selector.
- `BranchName`: Git branch associated with the documentation version.
- `IsLatestStable`: Selects the version used for unversioned documentation routes.
- `IsNextPrerelease`: Identifies the upcoming prerelease documentation.

Exactly one entry must be marked as the latest stable version and exactly one as the next prerelease. The same entry cannot hold both roles.

### Class reference synchronization

The Classes section is generated from the XML class reference in the Redot Engine repository. At startup, the application loads valid cached snapshots and starts a shallow, partial Git checkout of `doc/classes/*.xml` and `modules/*/doc_classes/*.xml` for every branch configured in `Versions.json` in the background. Snapshots are cached under `Redot-Documentation/App_Data/class-docs` and checked for upstream changes every 24 hours. If Git is temporarily unavailable, the application continues with the last valid cache; without a cache, class documentation remains unavailable until a synchronization succeeds.

The repository URL, source path, cache path, refresh interval, and Git timeout are configured in the `ClassDocumentation` section of `Redot-Documentation/appsettings.json`. Set `ClassDocumentation__Enabled=false` to disable synchronization for an offline development session. Git must be installed on the host.

Module class documentation is discovered automatically using wildcard sparse checkout; no per-module configuration or engine build is required. Core and module classes share the existing Classes index, search, and URLs. Only immediate `doc_classes/*.xml` files under each module are included, not unrelated XML or module source code. All selected XML is validated together before publication; duplicate class names or invalid module XML retain the last valid snapshot.

Existing core-only caches remain available while a one-time staged refresh adds module documentation, even if the engine commit is unchanged. Cache metadata records the selection revision and documentation directories so a missing module documentation directory triggers a repair. `ClassDocumentation:RepositoryPath` continues to configure the core XML directory; module discovery always uses `modules/*/doc_classes`.

`/health/class-docs` reports the active commit and class count for each documentation version and returns HTTP 503 if any configured version has no usable snapshot.

---

### Docker
The included Dockerfile installs Git and declares `/app/App_Data/class-docs` as the persistent class-reference cache volume. Persist that volume between container replacements to avoid downloading every configured branch after each deployment.

---

## License

The website implementation is licensed under [MIT](LICENSE.txt), beginning with
the revision introducing this license change. Earlier revisions retain their
existing licenses; previously granted CC permissions are not revoked.

Documentation in `Redot-Documentation/docs/` remains **CC BY 3.0 unless otherwise
noted**. Images and videos anywhere under `Redot-Documentation/wwwroot/` retain
their existing licenses. These are excluded from the website's MIT grant, as are
third-party libraries and assets.

The class reference is synchronized from [Redot Engine](https://github.com/Redot-Engine/redot-engine)
and retains its upstream MIT license and copyright notices.

See [LICENSING.md](LICENSING.md) for the complete scope, attribution, bundled
third-party notices, and contribution terms. The deployed site's `/licenses`
page provides the same distinctions and links to full license texts.

## Interface and theme

The site uses MudBlazor 9.9.0 with a shared Interactive Server root and prerendered
initial content. `Components/Layout/RedotTheme.cs` defines the Redot palette;
`wwwroot/app.css` styles the responsive home page, documentation layout, and HTML
produced by the Markdown and class-reference renderers. Bootstrap assets remain
in the repository with their notices but are no longer loaded by the site.

The home page uses the configured latest stable version for its guide and class
links. Getting Started content is included for each configured version; its
images live in `wwwroot/img/GettingStarted/`. Keep homepage links and imported
content references valid when changing versions or moving documentation.

MudBlazor providers live in `MainLayout`; nested navigation and viewer components
inherit the root render mode. Rendered Markdown stays HTML, with CSS styling and
small JavaScript helpers for code tabs and syntax highlighting.

### Documentation search

Search runs inside ASP.NET using Lucene.NET and BM25, without a crawler or external service. The header search button (Ctrl/Cmd+K) searches the selected version; `/search?q=collision&version=26.2&kind=all` is a shareable results page. Filters accept `all`, `guides`, or `classes`.

The background service indexes rendered Markdown sections and class-reference snapshots. Local indexes live under `App_Data/search/<version>/<content fingerprint>`; keep this writable directory on persistent storage to reuse indexes after restart. Fingerprints include source bytes, class revision, and a schema version. Class snapshot publication triggers a refresh; Markdown changes are picked up at application restart. Increment the search schema version when changing extraction or analysis. Index failures are logged and retain the previous searchable generation; first-time indexing displays a preparing state. Obsolete fingerprint directories are removed after the replacement reader and manifest are published. Cleanup failures are logged and retried after the next publication; temporary and unrelated directories are left untouched. Search logs section count, build duration, and disk size.

Results use literal term queries, prefix matching, and a one-edit typo fallback when no stronger matches exist. Queries are limited to 200 characters and 12 terms; results are grouped by source page. Search snippets retain the source content's existing license.
