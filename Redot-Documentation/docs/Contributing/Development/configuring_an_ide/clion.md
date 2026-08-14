
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

CLion does not support compiling and debugging Redot via SCons out of the box. This can be achieved by creating a custom build target and run configuration in CLion. Before creating a custom build target, you must [compile Redot](doc_introduction_to_the_buildsystem) once on the command line, to generate the Redot executable. Open the terminal, change into the Redot root directory, and execute:

```
scons dev_build=yes

```

To add a custom build target that invokes SCons for compilation:

- Open CLion and navigate to **Preferences > Build, Execution, Deployment > Custom Build Targets**

![Image](/img/Contributing/Development/configuring_an_ide/clion-preferences.png)

- Click **Add target** and give the target a name, e.g. ``Redot debug``.

![Image](/img/Contributing/Development/configuring_an_ide/clion-target.png)

- Click **...** next to the **Build:** selectbox, then click the **+** button in the **External Tools** dialog to add a new external tool.

![Image](/img/Contributing/Development/configuring_an_ide/clion-external-tools.png)

- Give the tool a name, e.g. ``Build Redot debug``, set **Program** to ``scons``, set **Arguments** to the compilation settings you want (see [compiling Redot](doc_introduction_to_the_buildsystem)), and set the **Working directory** to ``$ProjectFileDir$``, which equals the Redot root directory. Click **OK** to create the tool.

:::note

CLion does not expand shell commands like ``scons -j$(nproc)``. Use concrete values instead, e.g. ``scons -j8``.

:::

![Image](/img/Contributing/Development/configuring_an_ide/clion-create-build-tool.webp)

- Back in the **External Tools** dialog, click the **+** again to add a second external tool for cleaning the Redot build via SCons. Give the tool a name, e.g. ``Clean Redot debug``, set **Program** to ``scons``, set **Arguments** to ``-c`` (which will clean the build), and set the **Working directory** to ``$ProjectFileDir$``. Click **OK** to create the tool.

![Image](/img/Contributing/Development/configuring_an_ide/clion-create-clean-tool.png)

- Close the **External Tools** dialog. In the **Custom Build Target** dialog for the custom ``Redot debug`` build target, select the **Build Redot debug** tool from the **Build** select box, and select the **Clean Redot debug** tool from the **Clean** select box. Click **OK** to create the custom build target.

![Image](/img/Contributing/Development/configuring_an_ide/clion-select-tools.png)

- In the main IDE window, click **Add Configuration**.

![Image](/img/Contributing/Development/configuring_an_ide/clion-add-configuration.png)

- In the **Run/Debug Configuration** dialog, click **Add new...**, then select **Custom Build Application** to create a new custom run/debug configuration.

![Image](/img/Contributing/Development/configuring_an_ide/clion-add-custom-build-application.png)

- Give the run/debug configuration a name, e.g. ``Redot debug``, select the ``Redot debug`` custom build target as the **Target**. Select the Redot executable in the ``bin/`` folder as the **Executable**, and set the **Program arguments** to ``--editor --path path-to-your-project/``, where ``path-to-your-project/`` should be a path pointing to an existing Redot project. If you omit the ``--path`` argument, you will only be able to debug the Redot Project Manager window. Click **OK** to create the run/debug configuration.

![Image](/img/Contributing/Development/configuring_an_ide/clion-run-configuration.png)

You can now build, run, debug, profile, and Valgrind check the Redot editor via the run configuration.

![Image](/img/Contributing/Development/configuring_an_ide/clion-build-run.png)

When playing a scene, the Redot editor will spawn a separate process. You can debug this process in CLion by going to **Run > Attach to process...**, typing ``Redot``, and selecting the Redot process with the highest **pid** (process ID), which will usually be the running project.