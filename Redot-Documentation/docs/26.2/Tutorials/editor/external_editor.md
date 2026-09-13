
# Using an external text editor

This page explains how to code using an external text editor.

:::note

To code C# in an external editor, see
[the C# guide to configure an external editor ](doc_c_sharp_setup_external_editor).

:::

Redot can be used with an external text editor, such as Sublime Text or Visual
Studio Code. Browse to the relevant editor settings:
**Editor &gt; Editor Settings &gt; Text Editor &gt; External**

![Image](/img/Tutorials/editor/img/editor_external_editor_settings.webp)

   **Text Editor &gt; External** section of the Editor Settings

There are two text fields: the executable path and command-line flags. The flags
allow you to integrate the editor with Redot, passing it the file path to open
and other relevant arguments. Redot will replace the following placeholders in
the flags string:

| Field in Exec Flags | Is replaced with |
| --- | --- |
| ``{project}`` | The absolute path to the project directory |
| ``{file}`` | The absolute path to the file |
| ``{col}`` | The column number of the error |
| ``{line}`` | The line number of the error |

Some example **Exec Flags** for various editors include:

| Editor | Exec Flags |
| --- | --- |
| Geany/Kate | ``{file} --line {line} --column {col}`` |
| Atom | ``{file}:{line}`` |
| JetBrains Rider | ``{project} --line {line} {file}`` |
| Visual Studio Code | ``{project} --goto {file}:{line}:{col}`` |
| Vim (gVim) | ``"+call cursor({line}, {col})" {file}`` |
| Emacs | ``emacs +{line}:{col} {file}`` |
| Sublime Text | ``{project} {file}:{line}:{column}`` |

:::note

For Visual Studio Code on Windows, you will have to point to the ``code.cmd``
file.

For Emacs, you can call ``emacsclient`` instead of ``emacs`` if
you use the server mode.

:::

## Automatically reloading your changes
To have the Redot Editor automatically reload any script that has been changed by an external text editor,
enable **Editor &gt; Editor Settings &gt; Text Editor &gt; Behavior &gt; Auto Reload Scripts on External Change**.

## Using External Editor in Debugger

Using external editor in debugger is determined by a separate option in settings.
For details, see [Script editor debug tools and options ](doc_debugger_tools_and_options).

## Official editor plugins

We have official plugins for the following code editors:

- [Visual Studio Code ](https://github.com/redot-engine/redot-vscode-plugin)
- [Emacs ](https://github.com/redot-engine/emacs-gdscript-mode)

## LSP/DAP support

Redot supports the [Language Server Protocol ](https://microsoft.github.io/language-server-protocol/) (**LSP**) for code completion and the [Debug Adapter Protocol ](https://microsoft.github.io/debug-adapter-protocol/) (**DAP**) for debugging. You can check the [LSP client list ](https://microsoft.github.io/language-server-protocol/implementors/tools/) and [DAP client list ](https://microsoft.github.io/debug-adapter-protocol/implementors/tools/) to find if your editor supports them. If this is the case, you should be able to take advantage of these features without the need of a custom plugin.

To use these protocols, a Redot instance must be running on your current project. You should then configure your editor to communicate to the running adapter ports in Redot, which by default are ``6005`` for **LSP**, and ``6006`` for **DAP**. You can change these ports and other settings in the **Editor Settings**, under the **Network &gt; Language Server** and **Network &gt; Debug Adapter** sections respectively.

Below are some configuration steps for specific editors:

### Visual Studio Code

You need to install the official [Visual Studio Code plugin ](https://github.com/redot-engine/redot-vscode-plugin).

For **LSP**, follow [these instructions ](https://github.com/redot-engine/redot-vscode-plugin#gdscript_lsp_server_port) to change the default LSP port. The connection status can be checked on the status bar:

![Image](/img/Tutorials/editor/img/lsp_vscode_status.png)

For **DAP**, specify the ``debugServer`` property in your ``launch.json`` file:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "GDScript Redot",
            "type": "Redot",
            "request": "launch",
            "project": "${workspaceFolder}",
            "port": 6007,
            "debugServer": 6006,
        }
    ]
}

```

### Emacs

Check the official instructions to configure [LSP ](https://github.com/redot-engine/emacs-gdscript-mode#auto-completion-with-the-language-server-protocol-lsp), and [DAP ](https://github.com/redot-engine/emacs-gdscript-mode#using-the-debugger).