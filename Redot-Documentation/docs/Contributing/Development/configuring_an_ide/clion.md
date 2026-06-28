
# CLion

[CLion](https://www.jetbrains.com/clion/) is a commercial
[JetBrains](https://www.jetbrains.com/) IDE for C++.

## Importing the project

CLion can import a project's [compilation database file](https://clang.llvm.org/docs/JSONCompilationDatabase.html), commonly named ``compile_commands.json``. To generate the compilation database file, open the terminal, change to the Redot root directory, and run:

```
scons compiledb=yes

```

Then, open the Redot root directory with CLion. CLion will import the compilation database, index the codebase, and provide autocompletion and other advanced code navigation and refactoring functionality.

## Compiling and debugging the project

CLion does not support compiling and debugging Redot via SCons out of the box. This can be achieved by creating a custom build target and run configuration in CLion. Before creating a custom build target, you must [compile Redot](toc-devel-compiling) once on the command line, to generate the Redot executable. Open the terminal, change into the Redot root directory, and execute:

```
scons dev_build=yes

```

To add a custom build target that invokes SCons for compilation:

- Open CLion and navigate to **Preferences &gt; Build, Execution, Deployment &gt; Custom Build Targets**

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/clion-preferences.png" />
</figure>

- Click **Add target** and give the target a name, e.g. ``Redot debug``.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/clion-target.png" />
</figure>

- Click **...** next to the **Build:** selectbox, then click the **+** button in the **External Tools** dialog to add a new external tool.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/clion-external-tools.png" />
</figure>

- Give the tool a name, e.g. ``Build Redot debug``, set **Program** to ``scons``, set **Arguments** to the compilation settings you want (see [compiling Redot](toc-devel-compiling)), and set the **Working directory** to ``$ProjectFileDir$``, which equals the Redot root directory. Click **OK** to create the tool.

:::note
CLion does not expand shell commands like ``scons -j$(nproc)``. Use concrete values instead, e.g. ``scons -j8``.

:::

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/clion-create-build-tool.webp" />
</figure>

- Back in the **External Tools** dialog, click the **+** again to add a second external tool for cleaning the Redot build via SCons. Give the tool a name, e.g. ``Clean Redot debug``, set **Program** to ``scons``, set **Arguments** to ``-c`` (which will clean the build), and set the **Working directory** to ``$ProjectFileDir$``. Click **OK** to create the tool.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/clion-create-clean-tool.png" />
</figure>

- Close the **External Tools** dialog. In the **Custom Build Target** dialog for the custom ``Redot debug`` build target, select the **Build Redot debug** tool from the **Build** select box, and select the **Clean Redot debug** tool from the **Clean** select box. Click **OK** to create the custom build target.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/clion-select-tools.png" />
</figure>

- In the main IDE window, click **Add Configuration**.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/clion-add-configuration.png" />
</figure>

- In the **Run/Debug Configuration** dialog, click **Add new...**, then select **Custom Build Application** to create a new custom run/debug configuration.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/clion-add-custom-build-application.png" />
</figure>

- Give the run/debug configuration a name, e.g. ``Redot debug``, select the ``Redot debug`` custom build target as the **Target**. Select the Redot executable in the ``bin/`` folder as the **Executable**, and set the **Program arguments** to ``--editor --path path-to-your-project/``, where ``path-to-your-project/`` should be a path pointing to an existing Redot project. If you omit the ``--path`` argument, you will only be able to debug the Redot Project Manager window. Click **OK** to create the run/debug configuration.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/clion-run-configuration.png" />
</figure>

You can now build, run, debug, profile, and Valgrind check the Redot editor via the run configuration.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/clion-build-run.png" />
</figure>

When playing a scene, the Redot editor will spawn a separate process. You can debug this process in CLion by going to **Run &gt; Attach to process...**, typing ``Redot``, and selecting the Redot process with the highest **pid** (process ID), which will usually be the running project.