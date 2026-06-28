
# Qt Creator

[Qt Creator](https://doc.qt.io/qtcreator/index.html) is a free, open source IDE for all desktop platforms.

## Importing the project

- From the Qt Creator's main screen select **New Project &gt; Import Project &gt; Import Existing Project**.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/qtcreator-new-project.png" />
</figure>

- Under **Location** select the Redot root folder.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/qtcreator-set-project-path.png" />
</figure>

- Next, you can choose which folders and files will be visible to the project.
  While C/C++ files are added automatically, other extensions can be potentially useful:
  ``*.glsl`` for shader files, ``*.py`` for buildsystem files,
  ``*.java`` for Android platform development, ``*.mm`` for macOS platform development.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/qtcreator-apply-import-filter.png" />
</figure>

:::note
You can change this configuration later by right-clicking on your project
and selecting the **Edit Files...** option.

<figure>
  <img src="/img/qtcreator-edit-files-menu.png" />
</figure>

:::

- Finish the import.
- Open the ``project_name.includes`` file and add a line containing ``.`` to it
  to correctly enable the code completion.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/qtcreator-project-name-includes.png" />
</figure>

- From the left-side menu select **Projects** and open the **Build** tab.
- Delete the predefined ``make`` build step.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/qtcreator-projects-build.png" />
</figure>

- Click **Add Build Step &gt; Custom Process Step** to add a new build step
  with the following settings:

| Command | **scons** |
| --- | --- |
| Arguments | See [doc_introduction_to_the_buildsystem](../compiling/introduction_to_the_buildsystem.md) for a full list of arguments. |

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/qtcreator-set-scons-command.png" />
</figure>

:::note
If the build fails with ``Could not start process "scons"``, it can mean that ``scons``
is not in your ``PATH`` environment variable. In this case, you'll have to specify the
full path to the SCons binary.

:::

## Debugging the project

- From the left-side menu select **Projects** and open the **Run** tab.
- Under **Executable** specify the path to your executable located in
  the ``&lt;Redot root directory&gt;/bin`` folder. The name depends on your build configuration,
  e.g. ``Redot.linuxbsd.editor.dev.x86_64`` for 64-bit LinuxBSD platform with
  ``platform=editor`` and ``dev_build=yes``.
  You can use ``%{buildDir}`` to reference the project root, e.g: ``%{buildDir}/bin/Redot.linuxbsd.editor.dev.x86_64``.
- If you want to run a specific project, specify its root folder under **Working directory**.
- If you want to run the editor, add ``-e`` to the **Command line arguments** field.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/qtcreator-run-command.png" />
</figure>

To learn more about command line arguments, refer to the
[command line tutorial](../../../tutorials/editor/command_line_tutorial.md).

## Code style configuration

Developers must follow the project's [code style](../code_style_guidelines.md)
and the IDE should help them follow it. By default, Qt Creator uses spaces
for indentation which doesn't match the Redot code style guidelines. You can
change this behavior by changing the **Code Style** in **Tools &gt; Options &gt; C++**.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/qtcreator-options-cpp.png" />
</figure>

Click on **Edit** to change the current settings, then click on
**Copy Built-in Code Style** button to set a new code style. Set a name for it
(e.g. Redot) and change the Tab policy to be **Tabs Only**.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/qtcreator-edit-codestyle.png" />
</figure>

If you run into any issues, ask for help in one of
[Redot's community channels](https://redotengine.org/community).