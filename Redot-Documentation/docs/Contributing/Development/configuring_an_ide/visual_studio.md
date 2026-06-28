
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

<figure>
  <img src="/img/vs_1_install_cpp_package.png" />
</figure>

:::

## Debugging the project

Visual Studio features a powerful debugger. This allows the user to examine Redot's
source code, stop at specific points in the code, inspect the current execution context,
and make live changes to the codebase.

You can launch the project with the debugger attached using the **Debug &gt; Start Debugging**
option from the top menu. However, unless you want to debug the Project Manager specifically,
you'd need to configure debugging options first. This is due to the fact that when the Redot
Project Manager opens a project, the initial process is terminated and the debugger gets detached.

- To configure the launch options to use with the debugger use **Project &gt; Properties**
  from the top menu:

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/vs_2_project_properties.png" />
</figure>

- Open the **Debugging** section and under **Command Arguments** add two new arguments:
  the ``-e`` flag opens the editor instead of the Project Manager, and the ``--path`` argument
  tells the executable to open the specified project (must be provided as an *absolute* path
  to the project root, not the ``project.Redot`` file; if the path contains spaces be sure to pass it inside double quotation marks).

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/vs_3_debug_command_line.webp" />
</figure>

To learn more about command line arguments, refer to the
[command line tutorial](../../../tutorials/editor/command_line_tutorial.md).

Even if you start the project without a debugger attached it can still be connected to the running
process using **Debug &gt; Attach to Process...** menu.

To check that everything is working, put a breakpoint in ``main.cpp`` and press `F5` to
start debugging.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/vs_4_debugging_main.png" />
</figure>

If you run into any issues, ask for help in one of
[Redot's community channels](https://redotengine.org/community).