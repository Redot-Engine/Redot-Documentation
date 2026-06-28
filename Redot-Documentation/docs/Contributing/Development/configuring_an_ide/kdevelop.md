
# KDevelop

[KDevelop](https://www.kdevelop.org) is a free, open source IDE for all desktop platforms.

## Importing the project

- From the KDevelop's main screen select **Open Project**.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/kdevelop_newproject.png" alt="KDevelop's main screen." />
  <figcaption>
    KDevelop's main screen.
  </figcaption>
</figure>

- Navigate to the Redot root folder and select it.
- On the next screen, choose **Custom Build System** for the **Project Manager**.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/kdevelop_custombuild.png" />
</figure>

- After the project has been imported, open the project configuration by right-clicking
  on it in the **Projects** panel and selecting **Open Configuration..** option.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/kdevelop_openconfig.png" />
</figure>

- Under **Language Support** open the **Includes/Imports** tab and add the following paths:

```none
.  // A dot, to indicate the root of the Redot project
core/
core/os/
core/math/
drivers/
platform/<your_platform>/  // Replace <your_platform> with a folder
                              corresponding to your current platform

```

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/kdevelop_addincludes.png" />
</figure>

- Apply the changes.
- Under **Custom Build System** add a new build configuration with the following settings:

| Build Directory | *blank* |
| --- | --- |
| Enable | **True** |
| Executable | **scons** |
| Arguments | See [doc_introduction_to_the_buildsystem](../compiling/introduction_to_the_buildsystem.md) for a full list of arguments. |

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/kdevelop_buildconfig.png" />
</figure>

- Apply the changes and close the configuration window.

## Debugging the project

- Select **Run &gt; Configure Launches...** from the top menu.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/kdevelop_configlaunches.png" />
</figure>

- Click **Add** to create a new launch configuration.
- Select **Executable** option and specify the path to your executable located in
  the ``&lt;Redot root directory&gt;/bin`` folder. The name depends on your build configuration,
  e.g. ``Redot.linuxbsd.editor.dev.x86_64`` for 64-bit LinuxBSD platform with
  ``platform=editor`` and ``dev_build=yes``.

<figure>
  <img src="/Contributing/Development/configuring_an_ide/img/kdevelop_configlaunches2.png" />
</figure>

If you run into any issues, ask for help in one of
[Redot's community channels](https://redotengine.org/community).