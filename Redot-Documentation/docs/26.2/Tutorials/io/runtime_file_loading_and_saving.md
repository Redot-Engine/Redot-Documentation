
# Runtime file loading and saving

:::info

See [doc_saving_games](saving_games.md) for information on saving and loading game progression.

:::

Sometimes, [exporting packs, patches, and mods ](../export/exporting_pcks.md) is not
ideal when you want players to be able to load user-generated content in your
project. It requires users to generate a PCK or ZIP file through the Redot
editor, which contains resources imported by Redot.

Example use cases for runtime file loading and saving include:

- Loading texture packs designed for the game.
- Loading user-provided audio tracks and playing them back in an in-game radio station.
- Loading custom levels or 3D models that can be designed with any 3D DCC that
  can export to glTF (including glTF scenes saved by Redot at runtime).
- Using user-provided fonts for menus and HUD.
- Saving/loading a file format that can contain multiple files but can still
  easily be read by other applications (ZIP).
- Loading files created by another game or program, or even game data files from
  another game not made with Redot.

Runtime file loading can be combined with [HTTP requests ](../networking/http_request_class.md)
to load resources from the Internet directly.

:::warning

Do **not** use this runtime loading approach to load resources that are part
of the project, as it's less efficient and doesn't allow benefiting from
Redot's resource handling functionality (such as translation remaps). See
[doc_import_process](../assets_pipeline/import_process.md) for details.

:::

:::info

You can see how saving and loading works in action using the
[Run-time File Saving and Loading (Serialization) demo project ](https://github.com/redot-engine/redot-demo-projects/blob/master/loading/runtime_save_load).

:::

## Plain text and binary files

Redot's [class_FileAccess](class_FileAccess) class provides methods to access files on the
filesystem for reading and writing:

<Tabs>

<TabItem value="gdscript" label="Gdscript">

```gdscript
func save_file(content):
    var file = FileAccess.open("/path/to/file.txt", FileAccess.WRITE)
    file.store_string(content)

func load_file():
    var file = FileAccess.open("/path/to/file.txt", FileAccess.READ)
    var content = file.get_as_text()
    return content

```

</TabItem>

<TabItem value="csharp" label="Csharp">

```csharp
private void SaveFile(string content)
{
    using var file = FileAccess.Open("/Path/To/File.txt", FileAccess.ModeFlags.Write);
    file.StoreString(content);
}

private string LoadFile()
{
    using var file = FileAccess.Open("/Path/To/File.txt", FileAccess.ModeFlags.Read);
    string content = file.GetAsText();
    return content;
}

```

</TabItem>

</Tabs>

To handle custom binary formats (such as loading file formats not supported by
Redot), [class_FileAccess](class_FileAccess) provides several methods to read/write integers,
floats, strings and more. These FileAccess methods have names that start with
``get_`` and ``store_``.

If you need more control over reading binary files or need to read binary
streams that are not part of a file, [class_PackedByteArray](class_PackedByteArray) provides
several helper methods to decode/encode series of bytes to integers, floats,
strings and more. These PackedByteArray methods have names that start with
``decode_`` and ``encode_``. See also [doc_binary_serialization_api](binary_serialization_api.md).

## Images

Image's [Image.load_from_file ](class_Image_method_load_from_file) static method
handles everything, from format detection based on file extension to reading the
file from disk.

If you need error handling or more control (such as changing the scale an SVG is
loaded at), use one of the following methods depending on the file format:

- [Image.load_jpg_from_buffer ](class_Image_method_load_jpg_from_buffer)
- [Image.load_ktx_from_buffer ](class_Image_method_load_ktx_from_buffer)
- [Image.load_png_from_buffer ](class_Image_method_load_png_from_buffer)
- [Image.load_svg_from_buffer ](class_Image_method_load_svg_from_buffer)
  or [Image.load_svg_from_string ](class_Image_method_load_svg_from_string)
- [Image.load_tga_from_buffer ](class_Image_method_load_tga_from_buffer)
- [Image.load_webp_from_buffer ](class_Image_method_load_webp_from_buffer)

Several image formats can also be saved by Redot at runtime using the following
methods:

- [Image.save_png ](class_Image_method_save_png)
  or [Image.save_png_to_buffer ](class_Image_method_save_png_to_buffer)
- [Image.save_webp ](class_Image_method_save_webp)
  or [Image.save_webp_to_buffer ](class_Image_method_save_webp_to_buffer)
- [Image.save_jpg ](class_Image_method_save_jpg)
  or [Image.save_jpg_to_buffer ](class_Image_method_save_jpg_to_buffer)
- [Image.save_exr ](class_Image_method_save_exr)
  or [Image.save_exr_to_buffer ](class_Image_method_save_exr_to_buffer)
  *(only available in editor builds, cannot be used in exported projects)*

The methods with the ``to_buffer`` suffix save the image to a PackedByteArray
instead of the filesystem. This is useful to send the image over the network or
into a ZIP archive without having to write it on the filesystem. This can
increase performance by reducing I/O utilization.

:::note

If displaying the loaded image on a 3D surface, make sure to call
[Image.generate_mipmaps ](class_Image_method_generate_mipmaps)
so that the texture doesn't look grainy when viewed at a distance.
This is also useful in 2D when following instructions on
[reducing aliasing when downsampling ](doc_multiple_resolutions#reducing-aliasing-on-downsampling).

:::

Example of loading an image and displaying it in a [class_TextureRect](class_TextureRect) node
(which requires conversion to [class_ImageTexture](class_ImageTexture)):

<Tabs>

<TabItem value="gdscript" label="Gdscript">

```gdscript
# Load an image of any format supported by Redot from the filesystem.
var image = Image.load_from_file(path)
# Optionally, generate mipmaps if displaying the texture on a 3D surface
# so that the texture doesn't look grainy when viewed at a distance.
#image.generate_mipmaps()
$TextureRect.texture = ImageTexture.create_from_image(image)

# Save the loaded Image to a PNG image.
image.save_png("/path/to/file.png")

# Save the converted ImageTexture to a PNG image.
$TextureRect.texture.get_image().save_png("/path/to/file.png")

```

</TabItem>

<TabItem value="csharp" label="Csharp">

```csharp
// Load an image of any format supported by Redot from the filesystem.
var image = Image.LoadFromFile(path);
// Optionally, generate mipmaps if displaying the texture on a 3D surface
// so that the texture doesn't look grainy when viewed at a distance.
// image.GenerateMipmaps();
GetNode<TextureRect>("TextureRect").Texture = ImageTexture.CreateFromImage(image);

// Save the loaded Image to a PNG image.
image.SavePng("/Path/To/File.png");

// Save the converted ImageTexture to a PNG image.
GetNode<TextureRect>("TextureRect").Texture.GetImage().SavePng("/Path/To/File.png");

```

</TabItem>

</Tabs>

## Audio/video files

Redot supports loading Ogg Vorbis audio at runtime. Note that not *all* files
with an ``.ogg`` extension may be Ogg Vorbis files. Some may be Ogg Theora
videos, or contain Opus audio within an Ogg container. These files will **not**
load correctly as audio files in Redot.

Example of loading an Ogg Vorbis audio file in an [class_AudioStreamPlayer](class_AudioStreamPlayer) node:

<Tabs>

<TabItem value="gdscript" label="Gdscript">

```gdscript
$AudioStreamPlayer.stream = AudioStreamOggVorbis.load_from_file(path)

```

</TabItem>

<TabItem value="csharp" label="Csharp">

```csharp
GetNode<AudioStreamPlayer>("AudioStreamPlayer").Stream = AudioStreamOggVorbis.LoadFromFile(path);

```

</TabItem>

</Tabs>

Example of loading an Ogg Theora video file in a [class_VideoStreamPlayer](class_VideoStreamPlayer) node:

<Tabs>

<TabItem value="gdscript" label="Gdscript">

```gdscript
var video_stream_theora = VideoStreamTheora.new()
# File extension is ignored, so it is possible to load Ogg Theora videos
# that have an `.ogg` extension this way.
video_stream_theora.file = "/path/to/file.ogv"
$VideoStreamPlayer.stream = video_stream_theora

# VideoStreamPlayer's Autoplay property won't work if the stream is empty
# before this property is set, so call `play()` after setting `stream`.
$VideoStreamPlayer.play()

```

</TabItem>

<TabItem value="csharp" label="Csharp">

```csharp
var videoStreamTheora = new VideoStreamTheora();
// File extension is ignored, so it is possible to load Ogg Theora videos
// that have an `.ogg` extension this way.
videoStreamTheora.File = "/Path/To/File.ogv";
GetNode<VideoStreamPlayer>("VideoStreamPlayer").Stream = videoStreamTheora;

// VideoStreamPlayer's Autoplay property won't work if the stream is empty
// before this property is set, so call `Play()` after setting `Stream`.
GetNode<VideoStreamPlayer>("VideoStreamPlayer").Play();

```

</TabItem>

</Tabs>

:::note

Redot doesn't support runtime loading of MP3 or WAV files yet. Until this is
implemented, it's feasible to implement runtime WAV loading using a script
since [class_AudioStreamWAV](class_AudioStreamWAV)'s ``data`` property is exposed to
scripting.

It's still possible to *save* WAV files using
[AudioStreamWAV.save_to_wav ](class_AudioStreamWAV_method_save_to_wav), which is useful
for procedurally generated audio or microphone recordings.

:::

## 3D scenes

Redot has first-class support for glTF 2.0, both in the editor and exported
projects. Using [class_gltfdocument](class_gltfdocument) and [class_gltfstate](class_gltfstate) together,
Redot can load and save glTF files in exported projects, in both text
(``.gltf``) and binary (``.glb``) formats. The binary format should be preferred
as it's faster to write and smaller, but the text format is easier to debug.

Example of loading a glTF scene and appending its root node to the scene:

<Tabs>

<TabItem value="gdscript" label="Gdscript">

```gdscript
# Load an existing glTF scene.
# GLTFState is used by GLTFDocument to store the loaded scene's state.
# GLTFDocument is the class that handles actually loading glTF data into a Redot node tree,
# which means it supports glTF features such as lights and cameras.
var gltf_document_load = GLTFDocument.new()
var gltf_state_load = GLTFState.new()
var error = gltf_document_load.append_from_file("/path/to/file.gltf", gltf_state_load)
if error == OK:
    var gltf_scene_root_node = gltf_document_load.generate_scene(gltf_state_load)
    add_child(gltf_scene_root_node)
else:
    show_error("Couldn't load glTF scene (error code: %s)." % error_string(error))

# Save a new glTF scene.
var gltf_document_save := GLTFDocument.new()
var gltf_state_save := GLTFState.new()
gltf_document_save.append_from_scene(gltf_scene_root_node, gltf_state_save)
# The file extension in the output `path` (`.gltf` or `.glb`) determines
# whether the output uses text or binary format.
# `GLTFDocument.generate_buffer()` is also available for saving to memory.
gltf_document_save.write_to_filesystem(gltf_state_save, path)

```

</TabItem>

<TabItem value="csharp" label="Csharp">

```csharp
// Load an existing glTF scene.
// GLTFState is used by GLTFDocument to store the loaded scene's state.
// GLTFDocument is the class that handles actually loading glTF data into a Redot node tree,
// which means it supports glTF features such as lights and cameras.
var gltfDocumentLoad = new GltfDocument();
var gltfStateLoad = new GltfState();
var error = gltfDocumentLoad.AppendFromFile("/Path/To/File.gltf", gltfStateLoad);
if (error == Error.Ok)
{
    var gltfSceneRootNode = gltfDocumentLoad.GenerateScene(gltfStateLoad);
    AddChild(gltfSceneRootNode);
}
else
{
    GD.PrintErr($"Couldn't load glTF scene (error code: {error}).");
}

// Save a new glTF scene.
var gltfDocumentSave = new GltfDocument();
var gltfStateSave = new GltfState();
gltfDocumentSave.AppendFromScene(gltfSceneRootNode, gltfStateSave);
// The file extension in the output `path` (`.gltf` or `.glb`) determines
// whether the output uses text or binary format.
// `GltfDocument.GenerateBuffer()` is also available for saving to memory.
gltfDocumentSave.WriteToFilesystem(gltfStateSave, path);

```

</TabItem>

</Tabs>

:::note

When loading a glTF scene, a *base path* must be set so that external
resources like textures can be loaded correctly. When loading from a file,
the base path is automatically set to the folder containing the file. When
loading from a buffer, this base path must be manually set as there is no
way for Redot to infer this path.

To set the base path, set
[GLTFState.base_path ](class_GLTFState_property_base_path) on your
GLTFState instance *before* calling
[GLTFDocument.append_from_buffer ](class_GLTFDocument_method_append_from_buffer)
or [GLTFDocument.append_from_file ](class_GLTFDocument_method_append_from_file).

:::

## Fonts

[FontFile.load_dynamic_font ](class_FontFile_method_load_bitmap_font) supports the following
font file formats: TTF, OTF, WOFF, WOFF2, PFB, PFM

On the other hand, [FontFile.load_bitmap_font ](class_FontFile_method_load_bitmap_font) supports
the [BMFont ](https://www.angelcode.com/products/bmfont/) format (``.fnt`` or ``.font``).

Additionally, it is possible to load any font that is installed on the system using
Redot's support for [doc_using_fonts_system_fonts](doc_gui_using_fonts#system-fonts).

Example of loading a font file automatically according to its file extension,
then adding it as a theme override to a [class_Label](class_Label) node:

<Tabs>

<TabItem value="gdscript" label="Gdscript">

```gdscript
var path = "/path/to/font.ttf"
var path_lower = path.to_lower()
var font_file = FontFile.new()
if (
        path_lower.ends_with(".ttf")
        or path_lower.ends_with(".otf")
        or path_lower.ends_with(".woff")
        or path_lower.ends_with(".woff2")
        or path_lower.ends_with(".pfb")
        or path_lower.ends_with(".pfm")
):
    font_file.load_dynamic_font(path)
elif path_lower.ends_with(".fnt") or path_lower.ends_with(".font"):
    font_file.load_bitmap_font(path)
else:
    push_error("Invalid font file format.")

if not font_file.data.is_empty():
    # If font was loaded successfully, add it as a theme override.
    $Label.add_theme_font_override("font", font_file)

```

</TabItem>

<TabItem value="csharp" label="Csharp">

```csharp
string path = "/Path/To/Font.ttf";
var fontFile = new FontFile();

if (
    path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)
    || path.EndsWith(".otf", StringComparison.OrdinalIgnoreCase)
    || path.EndsWith(".woff", StringComparison.OrdinalIgnoreCase)
    || path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase)
    || path.EndsWith(".pfb", StringComparison.OrdinalIgnoreCase)
    || path.EndsWith(".pfm", StringComparison.OrdinalIgnoreCase)
)
{
    fontFile.LoadDynamicFont(path);
}
else if (path.EndsWith(".fnt", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".font", StringComparison.OrdinalIgnoreCase))
{
    fontFile.LoadBitmapFont(path);
}
else
{
    GD.PrintErr("Invalid font file format.");
}

if (!fontFile.Data.IsEmpty())
{
    // If font was loaded successfully, add it as a theme override.
    GetNode<Label>("Label").AddThemeFontOverride("font", fontFile);
}

```

</TabItem>

</Tabs>

## ZIP archives

Redot supports reading and writing ZIP archives using the [class_zipreader](class_zipreader)
and [class_zippacker](class_zippacker) classes. This supports any ZIP file, including files
generated by Redot's "Export PCK/ZIP" functionality (although these will contain
imported Redot resources rather than the original project files).

:::note

Use [ProjectSettings.load_resource_pack ](class_ProjectSettings_method_load_resource_pack)
to load PCK or ZIP files exported by Redot as
[additional data packs ](../export/exporting_pcks.md). That approach is preferred
for DLCs, as it makes interacting with additional data packs seamless (virtual filesystem).

:::

This ZIP archive support can be combined with runtime image, 3D scene and audio
loading to provide a seamless modding experience without requiring users to go
through the Redot editor to generate PCK/ZIP files.

Example that lists files in a ZIP archive in an [class_ItemList](class_ItemList) node,
then writes contents read from it to a new ZIP archive (essentially duplicating the archive):

<Tabs>

<TabItem value="gdscript" label="Gdscript">

```gdscript
# Load an existing ZIP archive.
var zip_reader = ZIPReader.new()
zip_reader.open(path)
var files = zip_reader.get_files()
# The list of files isn't sorted by default. Sort it for more consistent processing.
files.sort()
for file in files:
    $ItemList.add_item(file, null)
    # Make folders disabled in the list.
    $ItemList.set_item_disabled(-1, file.ends_with("/"))

# Save a new ZIP archive.
var zip_packer = ZIPPacker.new()
var error = zip_packer.open(path)
if error != OK:
    push_error("Couldn't open path for saving ZIP archive (error code: %s)." % error_string(error))
    return

# Reuse the above ZIPReader instance to read files from an existing ZIP archive.
for file in zip_reader.get_files():
    zip_packer.start_file(file)
    zip_packer.write_file(zip_reader.read_file(file))
    zip_packer.close_file()

zip_packer.close()

```

</TabItem>

<TabItem value="csharp" label="Csharp">

```csharp
// Load an existing ZIP archive.
var zipReader = new ZipReader();
zipReader.Open(path);
string[] files = zipReader.GetFiles();
// The list of files isn't sorted by default. Sort it for more consistent processing.
Array.Sort(files);
foreach (string file in files)
{
    GetNode<ItemList>("ItemList").AddItem(file);
    // Make folders disabled in the list.
    GetNode<ItemList>("ItemList").SetItemDisabled(-1, file.EndsWith('/'));
}

// Save a new ZIP archive.
var zipPacker = new ZipPacker();
var error = zipPacker.Open(path);
if (error != Error.Ok)
{
    GD.PrintErr($"Couldn't open path for saving ZIP archive (error code: {error}).");
    return;
}

// Reuse the above ZIPReader instance to read files from an existing ZIP archive.
foreach (string file in zipReader.GetFiles())
{
    zipPacker.StartFile(file);
    zipPacker.WriteFile(zipReader.ReadFile(file));
    zipPacker.CloseFile();
}

zipPacker.Close();
```

</TabItem>

</Tabs>
