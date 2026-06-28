
## Contributing code

The possibility to study, use, modify and redistribute modifications of the
engine's source code are the fundamental rights that
Redot's [MIT](https://tldrlegal.com/license/mit-license) license grants you,
making it [free and open source software](https://en.wikipedia.org/wiki/Free_and_open-source_software).

As such, everyone is entitled to modify
[Redot's source code](https://github.com/redot-engine/redot), and send those
modifications back to the upstream project in the form of a patch (a text file
describing the changes in a ready-to-apply manner) or - in the modern workflow
that we use - via a so-called "pull request" (PR), i.e. a proposal to directly
merge one or more Git commits (patches) into the main development branch.

Contributing code changes upstream has two big advantages:

-  Your own code will be reviewed and improved by other developers, and will be
   further maintained directly in the upstream project, so you won't have to
   reapply your own changes every time you move to a newer version. On the
   other hand it comes with a responsibility, as your changes have to be
   generic enough to be beneficial to all users, and not just your project; so
   in some cases it might still be relevant to keep your changes only for your
   own project, if they are too specific.

-  The whole community will benefit from your work, and other contributors will
   behave the same way, contributing code that will be beneficial to you. At
   the time of this writing, over 2,000 developers have contributed code
   changes to the engine!

To ensure good collaboration and overall quality, the Redot developers
enforce some rules for code contributions, for example regarding the style to
use in the C++ code (indentation, brackets, etc.) or the Git and PR workflow.

A good place to start is by searching for issues tagged as
[good first issue](https://github.com/redot-engine/redot-engine/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22)
on GitHub.

:::info
Technical details about the PR workflow are outlined in a
specific section, [doc_pr_workflow](pr_workflow.md).

Details about the code style guidelines and the ``clang-format``
tool used to enforce them are outlined in
[doc_code_style_guidelines](../Development/code_style_guidelines.md).

:::

All pull requests must go through a review process before being accepted.
Depending on the scope of the changes, it may take some time for a maintainer
responsible for the modified part of the engine to provide their review.
We value all of our contributors and ask them to be patient in the meantime,
as it is expected that in an open source project like Redot, there is going to be
way more contributions than people validating them.

To make sure that your time and efforts aren't wasted, it is recommended to vet the idea
first before implementing it and putting it for a review as a PR. To that end, Redot
has a [proposal system](https://github.com/redot-engine/redot-proposals). Its
usage is encouraged to plan changes and discuss them with the community. Implementation
details can also be discussed with other contributors on the [Redot Contributors Chat](https://chat.redotengine.org/).

:::note
Proposals are only required when working on an enhancement or a new feature.
Bug reports are sufficient for fixing issues.

:::

## Testing and reporting issues

Another great way of contributing to the engine is to test development releases
or the development branch and to report issues. It is also helpful to report
issues discovered in stable releases, so that they can be fixed in
the development branch and in future maintenance releases.

### Testing development versions

To help with the testing, you have several possibilities:

-  Compile the engine from source yourself, following the instructions of the
   [Compiling](toc-devel-compiling) page for your platform.

-  Test official pre-release binaries when they are announced (usually on the
   blog and other community platforms), such as alpha, beta and release candidate (RC) builds.

-  Test "trusted" unofficial builds of the development branch; just ask
   community members for reliable providers. Whenever possible, it's best to
   use official binaries or to compile yourself though, to be sure about the
   provenance of your binaries.

As mentioned previously, it is also helpful to keep your eyes peeled for
potential bugs that might still be present in the stable releases, especially
when using some niche features of the engine which might get less testing by
the developers.

### Filing an issue on GitHub

Redot uses [GitHub's issue tracker](https://github.com/redot-engine/redot-engine/issues)
for bug reports. When you start filing a bug report, you’ll be given a form to
fill out. Please try to follow it so that all issues are consistent and provide
the required information.

## Contributing to the documentation

There are two separate resources referred to as "documentation" in Redot:

- **The class reference.** This is the documentation for the complete Redot API
  as exposed to GDScript and the other scripting languages. It can be consulted
  offline, directly in Redot's code editor, or online at Redot :ref:`Class Reference
  &lt;doc_class_reference&gt;`. To contribute to the class reference, you have to edit the
  XML file corresponding to the class and make a pull request.
  See [doc_updating_the_class_reference](../Documentation/updating_the_class_reference.md) and [doc_class_reference_primer](../Documentation/class_reference_primer.md)
  for more details.

- **The tutorials and engine documentation and its translations.**
  This is the part you are reading now, which is distributed in the HTML format.
  Its contents are generated from plain text files in the reStructured Text
  (rst) format, to which you can contribute via pull requests on the
  [Redot-docs](https://github.com/redot-engine/redot-docs-site) GitHub repository.
  See [doc_contributing_to_the_documentation](../Documentation/contributing_to_the_documentation.md) for more details.

## Contributing translations

To make Redot accessible to everyone, including users who may prefer resources
in their native language instead of English, our community helps translate both
the Redot editor and its documentation in many languages.

See [doc_editor_and_docs_localization](../Documentation/editor_and_docs_localization.md) for more details.