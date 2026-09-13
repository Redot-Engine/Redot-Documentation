
# File paths in Redot projects

This page explains how file paths work inside Redot projects. You will learn how
to access paths in your projects using the ``res://`` and ``user://`` notations,
and where Redot stores project and editor files on your and your users' systems.

## Path separators

To make supporting multiple platforms easier, Redot uses **UNIX-style path
separators** (forward slash ``/``). These work on all platforms, **including
Windows**.

Instead of writing paths like ``C:\Projects\Game``, in Redot, you should write
``C:/Projects/Game``.

Windows-style path separators (backward slash ``\``) are also supported in some
path-related methods, but they need to be doubled (``\\``), as ``\`` is normally
used as an escape for characters with a special meaning.

This makes it possible to work with paths returned by other Windows
applications. We still recommend using only forward slashes in your own code to
guarantee that everything will work as intended.

:::tip

The String class offers over a dozen methods to work with strings that represent file paths:

- [String.filecasecmp_to() ](class_String_method_filecasecmp_to)
- [String.filenocasecmp_to() ](class_String_method_filenocasecmp_to)
- [String.get_base_dir() ](class_String_method_get_base_dir)
- [String.get_basename() ](class_String_method_get_basename)
- [String.get_extension() ](class_String_method_get_extension)
- [String.get_file() ](class_String_method_get_file)
- [String.is_absolute_path() ](class_String_method_is_absolute_path)
- [String.is_relative_path() ](class_String_method_is_relative_path)
- [String.is_valid_filename() ](class_String_method_is_valid_filename)
- [String.path_join() ](class_String_method_path_join)
- [String.simplify_path() ](class_String_method_simplify_path)
- [String.validate_filename() ](class_String_method_validate_filename)

:::

## Accessing files in the project folder (``res://``)

Redot considers that a project exists in any folder that contains a
``project.Redot`` text file, even if the file is empty. The folder that contains
this file is your project's root folder.

You can access any file relative to it by writing paths starting with
``res://``, which stands for resources. For example, you can access an image
file ``character.png`` located in the project's root folder in code with the
following path: ``res://character.png``.

## Accessing persistent user data (``user://``)

To store persistent data files, like the player's save or settings, you want to
use ``user://`` instead of ``res://`` as your path's prefix. This is because
when the game is running, the project's file system will likely be read-only.

The ``user://`` prefix points to a different directory on the user's device.
Unlike ``res://``, the directory pointed at by ``user://`` is created
automatically and *guaranteed* to be writable to, even in an exported project.

The location of the ``user://`` folder depends on what is configured in the
Project Settings:

- By default, the ``user://`` folder is created within Redot's
  [editor data path ](doc_data_paths_editor_data_paths) in the
  ``app_userdata/[project_name]`` folder. This is the default so that prototypes
  and test projects stay self-contained within Redot's data folder.
- If [application/config/use_custom_user_dir ](class_ProjectSettings_property_application/config/use_custom_user_dir)
  is enabled in the Project Settings, the ``user://`` folder is created **next
  to** Redot's editor data path, i.e. in the standard location for applications
  data.

  * By default, the folder name will be inferred from the project name, but it
    can be further customized with
    [application/config/custom_user_dir_name ](class_ProjectSettings_property_application/config/custom_user_dir_name).
    This path can contain path separators, so you can use it e.g. to group
    projects of a given studio with a ``Studio Name/Game Name`` structure.

On desktop platforms, the actual directory paths for ``user://`` are:

| Type | Location |
| --- | --- |
| Default |  | Windows: ``%APPDATA%\Redot\app_userdata\[project_name]`` macOS: ``~/Library/Application Support/Redot/app_userdata/[project_name]`` Linux: ``~/.local/share/Redot/app_userdata/[project_name]`` |
| Custom dir |  | Windows: ``%APPDATA%\[project_name]`` macOS: ``~/Library/Application Support/[project_name]`` Linux: ``~/.local/share/[project_name]`` |
| Custom dir and name |  | Windows: ``%APPDATA%\[custom_user_dir_name]`` macOS: ``~/Library/Application Support/[custom_user_dir_name]`` Linux: ``~/.local/share/[custom_user_dir_name]`` |

``[project_name]`` is based on the application name defined in the Project Settings, but
you can override it on a per-platform basis using [feature tags ](../export/feature_tags.md).

On mobile platforms, this path is unique to the project and is not accessible
by other applications for security reasons.

On HTML5 exports, ``user://`` will refer to a virtual filesystem stored on the
device via IndexedDB. (Interaction with the main filesystem can still be performed
through the [JavaScriptBridge ](class_JavaScriptBridge) singleton.)

## Converting paths to absolute paths or "local" paths

You can use [ProjectSettings.globalize_path() ](class_ProjectSettings_method_globalize_path)
to convert a "local" path like ``res://path/to/file.txt`` to an absolute OS path.
For example, [ProjectSettings.globalize_path() ](class_ProjectSettings_method_globalize_path)
can be used to open "local" paths in the OS file manager
using [OS.shell_open() ](class_OS_method_shell_open) since it only accepts
native OS paths.

To convert an absolute OS path to a "local" path starting with ``res://``
or ``user://``, use [ProjectSettings.localize_path() ](class_ProjectSettings_method_localize_path).
This only works for absolute paths that point to files or folders in your
project's root or ``user://`` folders.

## Editor data paths

The editor uses different paths for editor data, editor settings, and cache,
depending on the platform. By default, these paths are:

| Type | Location |
| --- | --- |
| Editor data |  | Windows: ``%APPDATA%\Redot\`` macOS: ``~/Library/Application Support/Redot/`` Linux: ``~/.local/share/Redot/`` |
| Editor settings |  | Windows: ``%APPDATA%\Redot\`` macOS: ``~/Library/Application Support/Redot/`` Linux: ``~/.config/Redot/`` |
| Cache |  | Windows: ``%TEMP%\Redot\`` macOS: ``~/Library/Caches/Redot/`` Linux: ``~/.cache/Redot/`` |

- **Editor data** contains export templates and project-specific data.
- **Editor settings** contains the main editor settings configuration file as
  well as various other user-specific customizations (editor layouts, feature
  profiles, script templates, etc.).
- **Cache** contains data generated by the editor, or stored temporarily.
  It can safely be removed when Redot is closed.

Redot complies with the [XDG Base Directory Specification](https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html)
on Linux/\*BSD. You can override the ``XDG_DATA_HOME``, ``XDG_CONFIG_HOME`` and
``XDG_CACHE_HOME`` environment variables to change the editor and project data
paths.

:::note
If you use [Redot packaged as a Flatpak](https://flathub.org/apps/details/org.godotengine.Redot), the
editor data paths will be located in subfolders in
``~/.var/app/org.godotengine.Redot/``.

:::

### Self-contained mode

If you create a file called ``._sc_`` or ``_sc_`` in the same directory as the
editor binary (or in `MacOS/Contents/` for a macOS editor .app bundle), Redot
will enable *self-contained mode*.
This mode makes Redot write all editor data, settings, and cache to a directory
named ``editor_data/`` in the same directory as the editor binary.
You can use it to create a portable installation of the editor.


:::note

Self-contained mode is not supported in exported projects yet.
To read and write files relative to the executable path, use
[OS.get_executable_path() ](class_OS_method_get_executable_path).
Note that writing files in the executable path only works if the executable
is placed in a writable location (i.e. **not** Program Files or another
directory that is read-only for regular users).

:::
