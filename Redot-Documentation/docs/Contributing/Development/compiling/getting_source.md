
# Getting the source

.. highlight:: shell

## Downloading the Redot source code

Before [getting into the SCons build system](introduction_to_the_buildsystem.md)
and compiling Redot, you need to actually download the Redot source code.

The source code is available on [GitHub](https://github.com/redot-engine/redot-engine)
and while you can manually download it via the website, in general you want to
do it via the ``git`` version control system.

If you are compiling in order to make contributions or pull requests, you should
follow the instructions from the [Pull Request workflow](../../Workflow/pr_workflow.md).

If you don't know much about ``git`` yet, there are a great number of
[tutorials](https://git-scm.com/book) available on various websites.

In general, you need to install ``git`` and/or one of the various GUI clients.

Afterwards, to get the latest development version of the Redot source code
(the unstable ``master`` branch), you can use ``git clone``.

If you are using the ``git`` command line client, this is done by entering
the following in a terminal:

```
git clone https://github.com/redot-engine/redot-engine.git
# You can add the --depth 1 argument to omit the commit history (shallow clone).
# A shallow clone is faster, but not all Git operations (like blame) will work.

```

For any stable release, visit the [release page](https://github.com/redot-engine/redot-engine/releases)
and click on the link for the release you want.
You can then download and extract the source from the download link on the page.

With ``git``, you can also clone a stable release by specifying its branch or tag
after the ``--branch`` (or just ``-b``) argument

```
# Clone the continuously maintained stable branch (`4.3` as of writing).
git clone https://github.com/redot-engine/Redot.git -b 4.3

# Clone the `4.3-stable` tag. This is a fixed revision that will never change.
git clone https://github.com/redot-engine/Redot.git -b 4.3-stable

# After cloning, optionally go to a specific commit.
# This can be used to access the source code at a specific point in time,
# e.g. for development snapshots, betas and release candidates.
cd Redot
git checkout f4af8201bac157b9d47e336203d3e8a8ef729de2

```

The [maintenance branches](https://github.com/redot-engine/redot-engine/branches/all)
are used to release further patches on each minor version.

You can get the source code for each release and pre-release in ``.tar.xz`` format from
[Redotengine/Redot-builds on GitHub](https://github.com/redot-engine/Redot-builds/releases).
This lacks version control information but has a slightly smaller download size.

After downloading the Redot source code,
you can [continue to compiling Redot](introduction_to_the_buildsystem.md).