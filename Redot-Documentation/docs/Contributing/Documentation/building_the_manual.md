import Tabs from "@theme/Tabs";
import TabItem from "@theme/TabItem";

# Building the manual with Sphinx

This page explains how to build a local copy of the Redot manual using the
Sphinx docs engine. This allows you to have local HTML files and build the
documentation as a PDF, EPUB, or LaTeX file, for example.

Before you get started, make sure that you have:

- [Git](https://git-scm.com/)
- [make](https://www.gnu.org/software/make/) (unless you're using Windows)
- [Python](https://www.python.org/) 3

:::note
Python 3 should come with the ``pip3`` command. You may need to write
``python3 -m pip`` (Unix) or  ``py -m pip`` (Windows) instead of ``pip3``.
If both approaches fail, `make sure that you have pip3 installed
&lt;https://pip.pypa.io/en/stable/installation/&gt;`__.

:::

1.  *(Optional)* Set up a virtual environment. Virtual environments prevent
    potential conflicts between the Python packages in ``requirements.txt`` and
    other Python packages that are installed on your system.

    a.  Create the virtual environment:

<Tabs>

<TabItem value="windows" label="Windows">

```pwsh
py -m venv Redot-docs-venv
```

</TabItem>

<TabItem value="other_platforms" label="Other platforms">

```sh
python3 -m venv Redot-docs-venv
```

</TabItem>

</Tabs>

    b.  Activate the virtual environment:

<Tabs>

<TabItem value="windows" label="Windows">

```pwsh
Redot-docs-venv\Scripts\activate.bat
```

</TabItem>

<TabItem value="other_platforms" label="Other platforms">

```sh
source Redot-docs-venv/bin/activate
```

</TabItem>

</Tabs>

    c.  *(Optional)* Update pre-installed packages:

<Tabs>

<TabItem value="windows" label="Windows">

```pwsh
py -m pip install --upgrade pip setuptools
```

</TabItem>

<TabItem value="other_platforms" label="Other platforms">

```sh
pip3 install --upgrade pip setuptools
```

</TabItem>

</Tabs>

2.  Clone the docs repo:

```sh
git clone https://github.com/redot-engine/redot-docs-site.git

```

3.  Change directory into the docs repo:

```sh
cd Redot-docs

```

4.  Install the required packages:

```sh
pip3 install -r requirements.txt

```

5.  Build the docs:

```sh
make html

```

:::note

On Windows, that command will run ``make.bat`` instead of GNU Make (or an alternative).

:::

    Alternatively, you can build the documentation by running the sphinx-build program manually:

```sh
sphinx-build -b html ./ _build/html

```

The compilation will take some time as the ``classes/`` folder contains hundreds of files.
See [doc_building_the_manual:performance](doc_building_the_manual:performance).

You can then browse the documentation by opening ``_build/html/index.html`` in
your web browser.

## Dealing with errors

If you run into errors, you may try the following command:

```sh
make SPHINXBUILD=~/.local/bin/sphinx-build html

```

If you get a ``MemoryError`` or ``EOFError``, you can remove the ``classes/`` folder and
run ``make`` again.
This will drop the class references from the final HTML documentation, but will keep the
rest intact.

:::info

If you delete the ``classes/`` folder, do not use ``git add .`` when working on a pull
request or the whole ``classes/`` folder will be removed when you commit.
See [#3157](https://github.com/redot-engine/redot-docs-site/issues/3157) for more detail.

:::

## Hints for performance

### RAM usage

Building the documentation requires at least 8 GB of RAM to run without disk swapping,
which slows it down.
If you have at least 16 GB of RAM, you can speed up compilation by running:

<Tabs>

<TabItem value="windows" label="Windows">

```pwsh
set SPHINXOPTS=-j2 && make html
```

</TabItem>

<TabItem value="other_platforms" label="Other platforms">

```sh
make html SPHINXOPTS=-j2
```

</TabItem>

</Tabs>

You can use ``-j auto`` to use all available CPU threads, but this can use a lot
of RAM if you have a lot of CPU threads. For instance, on a system with 32 CPU
threads, ``-j auto`` (which corresponds to ``-j 32`` here) can require 20+ GB of
RAM for Sphinx alone.

### Specifying a list of files

You can specify a list of files to build, which can greatly speed up compilation:

```sh
make html FILELIST='classes/class_node.rst classes/class_resource.rst'
```
