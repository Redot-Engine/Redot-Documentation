:::warning
This page is marked as outdated and may not reflect current Redot behavior.
:::

# HTML5 shell class reference

Projects exported for the Web expose the :js:class:`Engine` class to the JavaScript environment, that allows
fine control over the engine's start-up process.

This API is built in an asynchronous manner and requires basic understanding
of [Promises ](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Using_promises).

## Engine

The ``Engine`` class provides methods for loading and starting exported projects on the Web. For default export
settings, this is already part of the exported HTML page. To understand practical use of the ``Engine`` class,
see [Custom HTML page for Web export ](customizing_html5_shell.md).

### Static Methods

| Promise | :js:attr:`load &lt;Engine.load&gt;` **(** string basePath **)** |
| --- | --- |
| void | :js:attr:`unload &lt;Engine.unload&gt;` **(** **)** |
| boolean | :js:attr:`isWebGLAvailable &lt;Engine.isWebGLAvailable&gt;` **(** *[ number majorVersion=1 ]* **)** |

### Instance Methods

| Promise | :js:attr:`init &lt;Engine.prototype.init&gt;` **(** *[ string basePath ]* **)** |
| --- | --- |
| Promise | :js:attr:`preloadFile &lt;Engine.prototype.preloadFile&gt;` **(** string\ | ArrayBuffer file *[, string path ]* **)** |
| Promise | :js:attr:`start &lt;Engine.prototype.start&gt;` **(** EngineConfig override **)** |
| Promise | :js:attr:`startGame &lt;Engine.prototype.startGame&gt;` **(** EngineConfig override **)** |
| void | :js:attr:`copyToFS &lt;Engine.prototype.copyToFS&gt;` **(** string path, ArrayBuffer buffer **)** |
| void | :js:attr:`requestQuit &lt;Engine.prototype.requestQuit&gt;` **(** **)** |

## Engine configuration

An object used to configure the Engine instance based on Redot export options, and to override those in custom HTML
templates if needed.

### Properties

| type | name |
| --- | --- |
| boolean | :js:attr:`unloadAfterInit` |
| HTMLCanvasElement | :js:attr:`canvas` |
| string | :js:attr:`executable` |
| string | :js:attr:`mainPack` |
| string | :js:attr:`locale` |
| number | :js:attr:`canvasResizePolicy` |
| Array.&lt;string&gt; | :js:attr:`args` |
| function | :js:attr:`onExecute` |
| function | :js:attr:`onExit` |
| function | :js:attr:`onProgress` |
| function | :js:attr:`onPrint` |
| function | :js:attr:`onPrintError` |

