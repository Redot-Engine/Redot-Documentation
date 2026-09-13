# Licensing and attribution

## Website implementation and effective revision

The original ASP.NET/Blazor website implementation, tests, and build and
configuration code are offered under the [MIT license](LICENSE.txt), beginning
with the revision that introduces this license change. This includes the
implementation as distributed in that revision and subsequent changes to it.
Earlier revisions retain their existing licenses. No previously granted
Creative Commons permissions are revoked, and this change does not relicense
third-party works or documentation adaptations under MIT.

Copyright in contributions remains with their respective holders. The
maintainer has confirmed authorization for the original website code and written
relicensing consent from Decryptedchaos for their UI contributions.

## Documentation and media

`Redot-Documentation/docs/` is excluded from the website MIT grant. Unless a
more specific notice applies, its content is licensed under Creative Commons
Attribution 3.0 Unported ([CC BY 3.0](https://creativecommons.org/licenses/by/3.0/)).
The original, complete license text is preserved in
[docs/LICENSE.txt](Redot-Documentation/docs/LICENSE.txt).

Attribution: **the Redot community, modified from an original work by Juan
Linietsky, Ariel Manzur and the Godot community**. The original work is
[Godot Engine documentation](https://docs.godotengine.org/), whose
[source repository](https://github.com/godotengine/godot-docs) supplies the
upstream licensing notice. See also [GODOT_AUTHORS.md](GODOT_AUTHORS.md) and
[REDOT_AUTHORS.md](REDOT_AUTHORS.md); these documentation attribution records
remain under CC BY 3.0 and are not included in the website MIT grant.

Redot adapted this documentation for its engine and converted its format from
Sphinx/reStructuredText to Markdown rendered by ASP.NET/Blazor. Further edits
are recorded in repository history. This attribution does not imply 
endorsement by the original authors.

Images and videos **anywhere** under `Redot-Documentation/wwwroot/` are also
excluded from the website MIT grant, including `img/`, `video/`, `Icons/`,
`docs_logo.svg`, and `favicon.png`. Media imported from the documentation retains
CC BY 3.0 unless otherwise noted; separately licensed assets retain their own
terms. Existing credits in pages, captions, SVG metadata, and adjacent notices
must be preserved. The exclusion itself does not grant a new license to an
asset or resolve missing provenance. See
[the media notice](Redot-Documentation/wwwroot/MEDIA-LICENSES.txt).

## Engine class reference

The class reference is generated from `doc/classes/*.xml` and
`modules/*/doc_classes/*.xml` in
[Redot Engine](https://github.com/Redot-Engine/redot-engine), including copies
stored in the configurable class-documentation cache. It is separate from the
CC-licensed manual and retains the upstream MIT license:

- Copyright (c) 2024-present Redot Engine contributors (see upstream REDOT_AUTHORS.md).
- Copyright (c) 2014-present Godot Engine contributors (see upstream AUTHORS.md).
- Copyright (c) 2007-2014 Juan Linietsky, Ariel Manzur.

The [full upstream notice](Redot-Documentation/wwwroot/licenses/REDOT-ENGINE-MIT.txt)
is bundled with the site. Upstream author lists are available in the
[Redot](https://github.com/Redot-Engine/redot-engine/blob/master/REDOT_AUTHORS.md)
and [Godot](https://github.com/Redot-Engine/redot-engine/blob/master/AUTHORS.md)
files in that repository. Preserve any more specific source notices. If the
class source is changed to another repository, review and update its attribution
and license notices before distributing the resulting reference.

## Third-party software and assets

The website MIT grant does not replace any dependency's license.

| Bundled material | License and notice |
| --- | --- |
| `Redot-Documentation/wwwroot/lib/bootstrap/` (Bootstrap 5.3.3) | MIT, Copyright (c) 2011-2024 The Bootstrap Authors; [full text](Redot-Documentation/wwwroot/lib/bootstrap/LICENSE.txt) |
| `Redot-Documentation/wwwroot/lib/prism/` (Prism) | MIT, Copyright (c) 2012 Lea Verou; [full text](Redot-Documentation/wwwroot/lib/prism/LICENSE.txt) |
| `Redot-Documentation/wwwroot/Icons/Font-Awesome/` | Font Awesome Free: icons CC BY 4.0, fonts SIL OFL 1.1, code MIT, as applicable; [original notice](Redot-Documentation/wwwroot/Icons/Font-Awesome/LICENSE.txt) |
| MudBlazor 9.9.0 | MIT; [full notice](Redot-Documentation/wwwroot/licenses/MUDBLAZOR-MIT.txt) |
| Markdig 0.41.3 | BSD-2-Clause, Copyright (c) 2018-2019 Alexandre Mutel; [full text](Redot-Documentation/wwwroot/licenses/MARKDIG-BSD-2-CLAUSE.txt), retrieved from the package's source revision `7ff8db9016593b71f9ae17d9b2b053fbd54e9cdf` |
| .NET/ASP.NET Core and WebAssembly runtime assets | [MIT license](Redot-Documentation/wwwroot/licenses/DOTNET-MIT.txt), [.NET third-party notices](Redot-Documentation/wwwroot/licenses/DOTNET-THIRD-PARTY-NOTICES.txt), and [ASP.NET Core third-party notices](Redot-Documentation/wwwroot/licenses/ASPNETCORE-THIRD-PARTY-NOTICES.txt) |

The bundled .NET notices were copied from the restored
`Microsoft.NETCore.App.Runtime.Mono.browser-wasm` 10.0.1 and
`Microsoft.AspNetCore.Components.WebAssembly` 10.0.7 packages. Review these
notices when upgrading dependencies. Other packages, including development-only
test tools, retain their own package licenses.

Full license notices are included in publish output and Docker deployments.
The manual and class-reference viewers link to `/licenses`, which exposes the
applicable attribution and full texts without requiring access to GitHub.

## Contributions

Contribute website implementation changes under MIT; contribute manual changes
under CC BY 3.0 unless the affected material has a more specific license. New
media must include its source, author, license, and any modification notice in
the relevant page or an adjacent notice. Preserve third-party notices and do
not assume that an asset becomes MIT merely because it is used by the website.

The home page adapts the layout and descriptive text of the earlier Redot
Docusaurus documentation site. Its reused descriptive content and the imported
Getting Started guides and media retain CC BY 3.0 unless otherwise noted. The
new Razor implementation is MIT. The imported media retains its original notices.

Search uses Lucene.NET and Lucene.NET.Analysis.Common (Apache-2.0), J2N (see its bundled notices), and Html Agility Pack (MIT). Their license texts and notices are included under `Redot-Documentation/wwwroot/licenses/` and linked from the site license page.
