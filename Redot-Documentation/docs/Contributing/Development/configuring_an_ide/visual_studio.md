
# Visual Studio

[Visual Studio Community](https://visualstudio.microsoft.com) is a Windows-only IDE
by [Microsoft](https://microsoft.com) that's free for individual use or non-commercial use within organizations.
It has many useful features, such as memory view, performance view, source
control and more.

## Importing the project

Visual Studio requires a solution file to work on a project. While Redot does not come
with the solution file, it can be generated using SCons.

- Navigate to the Redot root folder and open a Command Prompt or PowerShell window.
- | Run ``scons platform=windows vsproj=yes dev_build=yes`` to generate the solution with debug symbols.
  | The ``vsproj`` parameter signals that you want Visual Studio solution generated.
  | The ``dev_build`` parameter makes sure the debug symbols are included, allowing to e.g. step through code using breakpoints.
- You can now open the project by double-clicking on the ``Redot.sln`` in the project root
  or by using the **Open a project or solution** option inside of the Visual Studio.
- Use the **Build** top menu to build the project.

:::warning

Visual Studio must be configured with the C++ package. It can be selected
in the installer:

![Image](/img/Contributing/Development/configuring_an_ide/vs_1_install_cpp_package.png)

:::

## Debugging the project

Visual Studio features a powerful debugger. This allows the user to examine Redot's
source code, stop at specific points in the code, inspect the current execution context,
and make live changes to the codebase.

You can launch the project with the debugger attached using the **Debug > Start Debugging**
option from the top menu. However, unless you want to debug the Project Manager specifically,
you'd need to configure debugging options first. This is due to the fact that when the Redot
Project Manager opens a project, the initial process is terminated and the debugger gets detached.

- To configure the launch options to use with the debugger use **Project > Properties**
  from the top menu:

![Image](/img/Contributing/Development/configuring_an_ide/vs_2_project_properties.png)

- Open the **Debugging** section and under **Command Arguments** add two new arguments:
  the ``-e`` flag opens the editor instead of the Project Manager, and the ``--path`` argument
  tells the executable to open the specified project (must be provided as an *absolute* path
  to the project root, not the ``project.Redot`` file; if the path contains spaces be sure to pass it inside double quotation marks).

![Image](/img/Contributing/Development/configuring_an_ide/vs_3_debug_command_line.webp)

<!-- TODO(Tekk): doc_command_line_tutorial doesnt exist -->
To learn more about command line arguments, refer to the
[command line tutorial](doc_command_line_tutorial).

Even if you start the project without a debugger attached it can still be connected to the running
process using **Debug > Attach to Process...** menu.

To check that everything is working, put a breakpoint in ``main.cpp`` and press `F5` to
start debugging.

![Image](/img/Contributing/Development/configuring_an_ide/vs_4_debugging_main.png)

If you run into any issues, ask for help in the
[Redot discord server](https://discord.com/invite/redot).