# Redot release policy

Redot's release policy is in constant evolution. The description below
provides a general idea of what to expect, but what will actually
happen depends on the choices of core contributors and the needs of the
community at a given time.

## Redot versioning

Starting in 2026, Redot has moved to a new versioning scheme with a ``year.quarter`` system.
We will release a new version of Redot every quarter, or every 3 months. Starting with 26.1, 
Redot is hard forked from Godot 4.5.2, and aims to be compatible with most Godot 4.5.2 projects and extensions.


:::note

In addition to stable releases, we also periodically release beta versions.
These are not intended for production use, but we encourage users to try them out and provide feedback.
These are versioned slightly differently from stable releases, with a ``-beta.<Build Number>`` suffix (e.g. ``26.2-beta.1``)
 or ``-rc.<Build Number>`` suffix (e.g. ``26.2-rc.1``).

:::

## Release support timeline

Starting in 2026, Redot will be released on a quarterly basis. The current version is supported until the next quarterly release.

| **Version**           | **Release date** | **Support level** |
|-----------------------|------------------|-------------------|
| Redot 26.3 (`master`) | TBD              | Unstable          |
| Redot 26.2            | June 2026        | Supported         |
| Redot 26.1            | Feburary 2026    | EOL               |
| Redot 4.4             | December 2025    | EOL               |
| Redot 4.3             | August 2024      | EOL               |

**Legend:**

|  **Status**  | **Description**          |
|:------------:|--------------------------|
|  Supported   | Full support             |
|     EOL      | No support (end of life) |
|   Unstable   | Development version      |

Pre-release Redot versions aren't intended to be used in production and are
provided for testing purposes only.

## Which version should I use for a new project?

We recommend using the latest stable build of Redot for new projects.
At the time of writing, this is Redot 26.2.

## Should I upgrade my project to use new engine versions?

:::note

Upgrading software while working on a project is inherently risky, so
consider whether it's a good idea for your project before attempting an
upgrade. Also, make backups of your project or use version control to
prevent losing data in case the upgrade goes wrong.

That said, we do our best to keep avoid breaking changes between releases.

:::

The general recommendation is to only upgrade your project if you are needing new features or improvements,
or if you are experiencing issues that have been fixed in a newer release.

## When is the next release out?

Redot Engine will typically put out a new release every quarter, or every three months.
Stable release will typically come towards the end of each quarter, with beta and release typically coming around the middle.

## What are the criteria for compatibility across engine versions?

:::note

This section is intended to be used by contributors to determine which
changes are safe for a given release. The list is not exhaustive; it only
outlines the most common situations encountered during Redot's development.

:::

The following changes are acceptable in releases:

- Fixing a bug in a way that has no major negative impact on most projects, such
  as a visual or physics bug. Redot's physics engine is not deterministic, so
  physics bug fixes are not considered to break compatibility. If fixing a bug
  has a negative impact that could impact a lot of projects, it should be made
  optional (e.g. using a project setting or separate method).
- Adding a new optional parameter to a method.
- Small-scale editor usability tweaks.
- New features that do not break existing projects.
- Deprecating a method, member variable, or class. This is done by adding a
  deprecated flag to its class reference, which will show up in the editor.

The following changes are considered **compatibility-breaking** and should be avoided:

- Renaming or removing a method, member variable, or class.
- Modifying a node's inheritance tree by making it inherit from a different class.
- Changing the default value of a project setting value in a way that affects existing
  projects. To only affect new projects, the project manager should write a
  modified ``project.Redot`` instead.


:::note

When modifying a method's signature in any fashion (including adding an
optional parameter), a GDExtension compatibility method must be created.
This ensures that existing GDExtensions continue to work across patch and
minor releases, so that users don't have to recompile them.
See [doc_handling_compatibility_breakages](../Contributing/Development/handling_compatibility_breakages.md) for more information.

:::
