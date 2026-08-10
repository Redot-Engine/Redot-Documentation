:::danger

This page is severely outdated and was written by the Godot team specifically for Sphinx.<br/>
This docs site is not using Sphinx, so none of the content on this page is currently applicable.<br/><br/>
However, this page still exists as a placeholder for now.<br/>
The new documentation site does not yet have the ability to sync the class reference with the main Redot repository.<br/>
The new site will eventually just pull the corresponding XML files from the Redot repository dynamically.<br/><br/>
We will update this page accordingly once that functionality is implemented.

:::

# Contributing to the class reference


The [Class reference](doc_class_reference) is a set of articles describing
the public API of the engine. This includes descriptions for various classes,
methods, properties, and global objects, available for scripting. The class reference
is available online, from the documentation sidebar, and in the Redot editor, from
the help menu.

As the engine grows and features are added or modified, some parts of the class
reference become obsolete and new descriptions and examples need to be added.
While developers are required to document their work in the class reference when
submitting a pull request, we can't expect every programmer to be a good
technical writer. There is always work for contributors like you to polish
existing and create missing reference material.

## The source of the class reference

As the class reference is available in two places, online and in the editor, we need to
take care to keep things in sync. To achieve this the [main Redot repository](https://github.com/Redot-Engine/redot-engine/)
is picked as the source of truth, and the documentation for the class reference is tracked there.

:::warning

You should **not** edit ``.rst`` files in the ``classes/`` folder of the
[documentation repository](https://github.com/Redot-Engine/Redot-Documentation).
These files are generated automatically and are synced manually by project
maintainers. Read further to learn how to correctly edit the class reference.

:::

In the main repository the class reference is stored in XML files, one for each exposed
class or global object. The majority of these files is located in [doc/classes/](https://github.com/redot-engine/redot-engine/tree/master/doc/classes),
but some modules contain their own documentation as well. You will find it in the ``modules/<module_name>/doc_classes/``
directory. To learn more about editing XML files refer to [doc_class_reference_primer](doc_class_reference_primer).

:::info

For details on Git usage and the pull request workflow, please
refer to the [doc_pr_workflow](doc_pr_workflow) page.

If you want to translate the class reference from English to another
language, see [doc_editor_and_docs_localization](editor_and_docs_localization.md). This guide is
also available as a [video tutorial on YouTube](https://www.youtube.com/watch?v=5jeHXxeX-JY).

:::

**Important:** If you plan to make large changes, you should create an issue on
the [Redot-Documentation repository](https://github.com/Redot-Engine/Redot-Documentation)
or comment on an existing issue. Doing so lets others know you're already
taking care of a given class.

## What to contribute

The natural place to start contributing is the classes that you are most familiar with.
This ensures that the added description will be based on experience and the necessary
know-how, not just the name of a method or a property. We advise not to add low effort
descriptions, no matter how appealing it may look. Such descriptions obscure the need
for documentation and are hard to identify automatically.

:::info

<!-- TODO(Tekk): i have no idea if we have a documentation status tracker... -->
Following this principle is important and allows us to create tools for contributors.
Such as the class reference's [completion status tracker](https://Redotengine.github.io/doc-status/).
You can use it to quickly find documentation pages missing descriptions.

:::

If you decide to document a class, but don't know what a particular method does, don't
worry. Leave it for now, and list the methods you skipped when you open a pull request
with your changes. Another writer will take care of it.

You can still look at the methods' implementation in Redot's source code on GitHub.
If you have doubts, feel free to ask on the [Redot Discord Server](https://discord.com/invite/redot).

:::warning

Unless you make minor changes, like fixing a typo, we do not recommend using the
GitHub web editor to edit the class reference's XML files. It lacks features to edit
XML well, like keeping indentations consistent, and it does not allow amending commits
based on reviews.

It also doesn't allow you to test your changes in the engine or with validation
scripts as described in [doc_class_reference_editing_xml](class_reference_primer#how-to-edit-class-xml).

:::

## Updating class reference when working on the engine

When you create a new class or modify an existing engine's API, you need to re-generate
the XML files in ``doc/classes/``.

To do so, you first need to compile Redot. See the [doc_introduction_to_the_buildsystem](doc_introduction_to_the_buildsystem)
page to learn how. Then, execute the compiled Redot binary from the Redot root directory
with the ``--doctool`` option. For example, if you're on 64-bit Linux, the command might be:

```
./bin/Redot.linuxbsd.editor.x86_64 --doctool

```

The exact set of suffixes may be different. Carefully read through the linked article to
learn more about that.

The XML files in ``doc/classes/`` should then be up-to-date with current Redot Engine
features. You can then check what changed using the ``git diff`` command.

Please only include changes that are relevant to your work on the API in your commits.
You can discard changes in other XML files using ``git checkout``, but consider reporting
if you notice unrelated files being updated. Ideally, running this command should only
bring up the changes that you yourself have made.

You will then need to add descriptions to any newly generated entries.
