
# Contributing to the documentation

This guide explains how to contribute to Redot's documentation, be it by
writing or reviewing pages.

:::info

If you want to translate pages or the class reference from English to other
languages, read [editor_and_docs_localization](doc_editor_and_docs_localization).

:::

## Getting started

To modify or create pages in the reference manual, you need to edit ``.md``
files in the [Redot-Documentation GitHub repository](https://github.com/redot-engine/redot-docs-site).
Modifying those pages in a pull request triggers a rebuild of the online documentation upon merging.

:::info
For details on Git usage and the pull request workflow, please
refer to the [PR Workflow](../Workflow/doc_pr_workflow) page. Most of what it describes
regarding the main Redotengine/Redot repository is also valid for
the docs repository.

:::

:::warning
The class reference's source files are in the [Redot engine repository](https://github.com/redot-engine/redot-engine). We generate
the [Class Reference](doc_class_reference) section of this documentation
from them. If you want to update the description of a class, its
methods, or properties, read
[updating_the_class_reference](doc_updating_the_class_reference).

:::

## What is the Redot documentation

The Redot documentation is intended as a comprehensive reference manual for the
Redot game engine. It is not meant to contain step-by-step tutorials, except for
two game creation tutorials in the Getting Started section.

We strive to write factual content in an accessible and well-written language. To
contribute, you should also read:

1. [docs_writing_guidelines](doc_docs_writing_guidelines). There, you will find rules and
   recommendations to write in a way that everyone understands.
2. [content_guidelines](doc_content_guidelines). They explain the principles we follow to write the
   documentation and the kind of content we accept.

## Contributing changes

**Pull requests should only use the** ``master`` **branch.**

### Editing existing pages

To edit an existing page, locate its ``.md`` source file and open it in your
favorite text editor. You can then commit the changes, push them to your fork,
and make a pull request. **Note that the pages in** ``classes/`` **should not be
edited here.** They are automatically generated from Redot's [XML class reference](https://github.com/redot-engine/redot-engine/tree/master/doc/classes).
See [updating_the_class_reference](doc_updating_the_class_reference) for details.

:::info
To build the manual and test changes on your computer, see
[building_the_manual](doc_building_the_manual).

:::

## Editing pages online

You can edit the documentation online by clicking the **Edit on GitHub** link in
the top-right of every page.

Doing so takes you to the GitHub text editor. You need to have a GitHub account
and to log in to use it. Once logged in, you can propose change like so:

1. Click the **Edit on GitHub** button.

2. On the GitHub page you're taken to, make sure the current branch is "master".
   Click the pencil icon in the top-right corner
   near the **Raw**, **Blame**, and **Delete** buttons.
   It has the tooltip "Fork this project and edit the file".

3. Edit the text in the text editor.

4. Click "Commit changes...", summarize the changes you made
   and make sure to replace the placeholder "Update file.rst" by a short,
   but clear one-line description, as this is the commit title.
   Click the button **Propose changes**.

5. On the following screens, click the **Create pull request** button until you
   see a message like *Username wants to merge 1 commit into Redot-engine:master
   from Username:patch-1*.

:::note

If there are more commits than your own in the pull request
it is likely that your branch was created using the wrong origin,
due to "master" not being the current branch in step 2.
You will need to rebase your branch to "master" or create a new branch.

:::

Another contributor will review your changes and merge them into the docs if
they're good. They may also make changes or ask you to do so before merging.

## Adding new pages

Before adding a new page, please ensure that it fits in the documentation:

1. Look for [existing issues](https://github.com/redot-engine/redot-docs-site/issues)
   or open a new one to see if the page is necessary.
2. Ensure there isn't a page that already covers the topic.
3. Read our [content_guidelines](doc_content_guidelines).

To add a new page, create a ``.md`` file with a meaningful name in the section you
want to add a file to, e.g. ``tutorials/3d/light_baking.md``.

You should then add your page to the relevant "toctree" (table of contents,
e.g. ``tutorials/3d/index.rst``). Add your new filename to the list on a new
line, using a relative path and no extension, e.g. here ``light_baking``.

### Titles

Always begin pages with their title:

```markdown

# Insert your title here


```

Most articles can be referenced in links by adding a prefix to their filename.
For most types of articles, the prefix should be ``doc_``, however, several others exist such as ``abt_``, and ``class_``,
which are for the about section and class reference pages respectively.

For example, let's say that you made an article in ``Contributing/Documentation/foo.md``.
You can link to it from any other page by using <code>[foo]</code><code>(doc_foo)</code>.

Write your titles like plain sentences, without capitalizing each word:

-  **Good:** Understanding signals in Redot
-  **Bad:** Understanding Signals In Redot

Only proper nouns, projects, people, and node class names should have their
first letter capitalized.

### Markdown syntax guide

The documentation in this repository is written in Markdown (``.md`` files).
Most pages can be written using standard Markdown syntax:

- Headings: ``#``, ``##``, ``###``
- Links: ``[link text](https://example.com)`` or ``[internal_link](doc_page_name)``
- Inline code: `` `code` ``
- Code blocks:

```markdown
```language
Your code here
```


For callouts such as notes, tips, and warnings, use the container blocks used
throughout this documentation:

<pre><code class="language-markdown">
:::<i></i>note
Your note here.
:::

:::<i></i>info
Useful context.
:::

:::<i></i>warning
Important warning.
:::
</code></pre>

These callouts will render like below.

:::note
Your note here.
:::

:::info
Useful context.
:::

:::warning
Important warning.
:::

### Adding images and attachments

To add images, please put them in the corresponding folder within ``wwwroot/img/<PATH TO IMAGE>`` file with
a meaningful name and include them in your page with:

```markdown
![Descriptive alt text](/img/image_name.webp)

```

If you need a captioned image block, you can use HTML in Markdown:

```markdown
<figure>
  <img src="img/image_name.webp" alt="Descriptive alt text" />
  <figcaption>Image caption text.</figcaption>
</figure>

```

You can also include attachments as support material for a tutorial, by placing them
into a ``files/`` folder next to the ``.md`` file, and using normal Markdown links:

Consider using the [Redot-docs-project-starters](https://github.com/redot-engine/redot-docs-site-project-starters)
repository for hosting support materials, such as project templates and asset packs.
You can use a direct link to the generated archive from that repository with the regular
link markup:

```markdown
[file_name.zip](https://github.com/redot-engine/redot-docs-site-project-starters/releases/download/latest-4.x/file_name.zip)

```

## License

This documentation and every page it contains is published under the terms of
the `Creative Commons Attribution 3.0 license (CC BY 3.0)
&lt;https://creativecommons.org/licenses/by/3.0/&gt;`_, with attribution to "Juan
Linietsky, Ariel Manzur and the Redot community".

By contributing to the documentation on the GitHub repository, you agree that
your changes are distributed under this license.