
# Compiling for iOS

.. highlight:: shell

:::info

This page describes how to compile iOS export template binaries from source.
If you're looking to export your project to iOS instead, read [doc_exporting_for_ios](doc_exporting_for_ios).

:::

## Requirements

- [Python 3.8+](https://www.python.org/downloads/macos/).
- [SCons 4.0+](https://scons.org/pages/download.html) build system.
- [Xcode](https://apps.apple.com/us/app/xcode/id497799835).
    - Launch Xcode once and install iOS support. If you have already launched
      Xcode and need to install iOS support, go to *Xcode -&gt; Settings... -&gt; Platforms*.
    - Go to *Xcode -&gt; Settings... -&gt; Locations -&gt; Command Line Tools* and select
      an installed version. Even if one is already selected, re-select it.
-  Download and follow README instructions to build a static ``.xcframework``
   from the [MoltenVK SDK](https://github.com/KhronosGroup/MoltenVK#fetching-moltenvk-source-code).

:::note
If you have [Homebrew](https://brew.sh/) installed, you can easily
install SCons using the following command

```
brew install scons

```

Installing Homebrew will also fetch the Command Line Tools
for Xcode automatically if you don't have them already.

Similarly, if you have [MacPorts](https://www.macports.org/)
installed, you can easily install SCons using the
following command

```
sudo port install scons
```

:::

:::info
To get the Redot source code for compiling, see
[doc_getting_source](doc_getting_source).

For a general overview of SCons usage for Redot, see
[doc_introduction_to_the_buildsystem](doc_introduction_to_the_buildsystem).

:::

## Compiling

Open a Terminal, go to the root folder of the engine source code and type
the following to compile a debug build:

```
scons platform=ios target=template_debug generate_bundle=yes

```

To compile a release build:

```
scons platform=ios target=template_release generate_bundle=yes

```

Alternatively, you can run the following command for Xcode simulator libraries (optional):

```
scons platform=ios target=template_debug ios_simulator=yes arch=arm64
scons platform=ios target=template_debug ios_simulator=yes arch=x86_64 generate_bundle=yes

```

These simulator libraries cannot be used to run the exported project on the
target device. Instead, they can be used to run the exported project directly on
your Mac while still testing iOS platform-specific functionality.

To create an Xcode project like in the official builds, you need to use the
template located in ``misc/dist/ios_xcode``. The release and debug libraries
should be placed in ``libRedot.ios.debug.xcframework`` and
``libRedot.ios.release.xcframework`` respectively. This process can be automated
by using the ``generate_bundle=yes`` option on the *last* SCons command used to
build export templates (so that all binaries can be included).

The MoltenVK static ``.xcframework`` folder must also be placed in the
``ios_xcode`` folder once it has been created. MoltenVK is always statically
linked on iOS; there is no dynamic linking option available, unlike macOS.

## Run

To run on a device or simulator, follow these instructions:
[doc_exporting_for_ios](doc_exporting_for_ios).