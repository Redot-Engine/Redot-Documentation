using System.Text.Json;
using Redot_Documentation.Search;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using MudBlazor.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Redot_Documentation.ClassDocumentation;
using Redot_Documentation.Components.Layout;
using Redot_Documentation.Components.Pages;
using Redot_Documentation.Services;
using Redot_Documentation.Versioning;

namespace Redot_Documentation_Tests;

// Exercise real component lifecycle and dispatcher behavior without a browser or extra test packages.
#pragma warning disable BL0006
public sealed class ClassDocumentationComponentTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"redot-component-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task Search_KeepsResultsAndHighlighting_AndRejectsOutOfOrderResponses()
    {
        var search = new ControlledSearch();
        await using var services = CreateServices(search);
        await using var renderer = new TestRenderer(services);
        int id = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderAsync(typeof(SearchPanel),
            ParameterView.FromDictionary(new Dictionary<string, object?> { ["Query"] = "alpha" })));
        var input = await renderer.Dispatcher.InvokeAsync(() => renderer.FindComponent<MudBlazor.MudTextField<string>>(id));
        Assert.NotNull(input);
        Task oldRequest = renderer.Dispatcher.InvokeAsync(() => input.ValueChanged.InvokeAsync("slow"));
        try
        {
            await search.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            string pendingText = await renderer.Dispatcher.InvokeAsync(() => renderer.Text(id));
            Assert.Contains("alpha result", pendingText);
            Assert.Contains("<mark>alpha</mark>", pendingText);
            string before = services.GetRequiredService<NavigationManager>().Uri;
            await renderer.Dispatcher.InvokeAsync(() => input.OnKeyDown.InvokeAsync(new KeyboardEventArgs { Key = "Enter" }));
            Assert.Equal(before, services.GetRequiredService<NavigationManager>().Uri);
            await renderer.Dispatcher.InvokeAsync(() => input.ValueChanged.InvokeAsync("beta"));
            Assert.Contains("beta result", await renderer.Dispatcher.InvokeAsync(() => renderer.Text(id)));
        }
        finally { search.Release.Set(); }
        await oldRequest;
        string finalText = await renderer.Dispatcher.InvokeAsync(() => renderer.Text(id));
        Assert.Contains("beta result", finalText);
        Assert.DoesNotContain("slow result", finalText);
        Assert.Contains("<mark>beta</mark>", finalText);
        await renderer.Dispatcher.InvokeAsync(() => input.ValueChanged.InvokeAsync(""));
        Assert.DoesNotContain("beta result", await renderer.Dispatcher.InvokeAsync(() => renderer.Text(id)));
        Assert.Empty(renderer.Errors);
    }

    [Fact]
    public async Task Search_UrlParameterEchoDoesNotRepeatCompletedQuery()
    {
        var search = new ControlledSearch();
        await using var services = CreateServices(search);
        await using var renderer = new TestRenderer(services);
        ParameterView Parameters(string query) => ParameterView.FromDictionary(new Dictionary<string, object?>
        { ["Query"] = query, ["Version"] = "26.1", ["Kind"] = "all", ["FullPage"] = true });
        int id = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderAsync(typeof(SearchPanel), Parameters("alpha")));
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var input = renderer.FindComponent<MudBlazor.MudTextField<string>>(id)!;
            await input.ValueChanged.InvokeAsync("beta");
            await renderer.UpdateAsync(id, Parameters("beta"));
        });
        Assert.Equal(2, search.Calls);
        Assert.Contains("beta result", await renderer.Dispatcher.InvokeAsync(() => renderer.Text(id)));
        Assert.Empty(renderer.Errors);
    }

    private sealed class ControlledSearch : IDocumentationSearch
    {
        public int Calls;
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public ManualResetEventSlim Release { get; } = new(false);
        public SearchResponse Search(string version, string query, string kind = "all", int limit = 30)
        {
            Interlocked.Increment(ref Calls);
            if (query == "slow") { Started.TrySetResult(); Release.Wait(TimeSpan.FromSeconds(10)); }
            return new(true, [new(query + " result", query, "/en/26.1/" + query, "guides", "alpha beta slow")]);
        }
    }

    [Fact]
    public async Task NavMenu_SelectsConfiguredVersionThroughMudSelect()
    {
        await using var services = CreateServices();
        await using var renderer = new TestRenderer(services);
        int id = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderAsync(typeof(NavMenu), ParameterView.Empty));
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var select = renderer.FindComponent<MudBlazor.MudSelect<string>>(id);
            var item = renderer.FindComponent<MudBlazor.MudSelectItem<string>>(id);
            Assert.NotNull(select);
            Assert.NotNull(item);
            Assert.Equal("26.1", select.GetState(x => x.Value));
            Assert.Equal("latest", item.Value);
            await select.ValueChanged.InvokeAsync(item.Value);
            Assert.Equal("latest", select.GetState(x => x.Value));
        });
        Assert.EndsWith("/en/latest/Classes", services.GetRequiredService<NavigationManager>().Uri);
        Assert.Empty(renderer.Errors);
    }

    [Fact]
    public async Task Home_LinksToExistingStableDocumentation()
    {
        await using var services = CreateServices();
        await using var renderer = new TestRenderer(services);
        int id = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderAsync(typeof(Home), ParameterView.Empty));
        string[] links = await renderer.Dispatcher.InvokeAsync(() => renderer.Hrefs(id).Where(h => h.StartsWith("/en/")).Distinct().ToArray());
        Assert.True(links.Length >= 10);
        DirectoryInfo? repository = new(AppContext.BaseDirectory);
        while (repository is not null && !File.Exists(Path.Combine(repository.FullName, "Redot-Documentation.sln")))
            repository = repository.Parent;
        Assert.NotNull(repository);
        foreach (string href in links)
        {
            Assert.DoesNotContain("/latest/", href);
            if (href.EndsWith("/Classes")) continue;
            string path = Path.Combine(repository.FullName, "Redot-Documentation", "docs", Uri.UnescapeDataString(href[4..]) + ".md");
            Assert.True(File.Exists(path), $"Homepage destination is missing: {href}");
        }
        Assert.Empty(renderer.Errors);
    }

    [Fact]
    public async Task ClassSearch_FiltersResultsThroughMudInput()
    {
        await using var services = CreateServices();
        var manager = services.GetRequiredService<VersionManagerService>();
        services.GetRequiredService<ClassDocumentationCatalog>().Publish(new(
            manager.LatestStableVersion, "test-revision", DateTimeOffset.UtcNow,
            new Dictionary<string, ClassDocumentationEntry>
            {
                ["Node"] = new() { Name = "Node" },
                ["Resource"] = new() { Name = "Resource" }
            }));
        await using var renderer = new TestRenderer(services);
        int id = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderAsync(typeof(ClassDocViewer),
            ParameterView.FromDictionary(new Dictionary<string, object?> { ["VersionSlug"] = "26.1" })));
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var input = renderer.FindComponent<MudBlazor.MudTextField<string>>(id);
            Assert.NotNull(input);
            await input.ValueChanged.InvokeAsync("Resource");
        });
        string text = await renderer.Dispatcher.InvokeAsync(() => renderer.Text(id));
        Assert.Contains("Resource", text);
        Assert.DoesNotContain("Node", text);
        Assert.Empty(renderer.Errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Node")]
    public async Task ClassReference_IncludesUpstreamMitAttribution(string? className)
    {
        await using var services = CreateServices();
        var manager = services.GetRequiredService<VersionManagerService>();
        services.GetRequiredService<ClassDocumentationCatalog>().Publish(new(
            manager.LatestStableVersion, "test-revision", DateTimeOffset.UtcNow,
            new Dictionary<string, ClassDocumentationEntry> { ["Node"] = new() { Name = "Node" } }));
        await using var renderer = new TestRenderer(services);
        int id = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderAsync(typeof(ClassDocViewer),
            ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                ["VersionSlug"] = "26.1",
                ["ClassName"] = className
            })));

        string text = await renderer.Dispatcher.InvokeAsync(() => renderer.Text(id));
        Assert.Contains("MIT license", text);
        Assert.Contains("Godot Engine contributors", text);
        Assert.Contains("Juan Linietsky, Ariel Manzur", text);
        Assert.DoesNotContain("CC BY 3.0", text);
        Assert.Empty(renderer.Errors);
    }

    [Theory]
    [InlineData("26.1/tutorials/MIXED.HTML")]
    [InlineData("26.1/TUTORIALS/mixed.MD")]
    [InlineData("tutorials/mixed")]
    public async Task Manual_LegacyAliasesRenderAndHeadingLinksPreserveQuery(string route)
    {
        Directory.CreateDirectory(Path.Combine(_root, "docs", "26.1", "Tutorials"));
        File.WriteAllText(Path.Combine(_root, "docs", "26.1", "Tutorials", "Mixed.md"), "# Mixed heading");
        await using var services = CreateServices();
        services.GetRequiredService<NavigationManager>().NavigateTo("/en/" + route + "?source=Legacy#Mixed");
        await using var renderer = new TestRenderer(services);
        int id = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderAsync(typeof(DocViewer),
            ParameterView.FromDictionary(new Dictionary<string, object?> { ["DocumentPath"] = route })));
        Assert.Contains("Mixed heading", await renderer.Dispatcher.InvokeAsync(() => renderer.Text(id)));
        Assert.Contains("/en/26.1/Tutorials/Mixed?source=Legacy#mixed-heading",
            await renderer.Dispatcher.InvokeAsync(() => renderer.Hrefs(id).ToArray()));
        Assert.Empty(renderer.Errors);
    }

    [Theory]
    [InlineData("/en/26.1/tutorials/MIXED.HTML?source=Legacy#SomeHeading", "/en/latest/Tutorials/Mixed?source=Legacy#SomeHeading")]
    [InlineData("/en/tutorials/mixed.md?source=Legacy#SomeHeading", "/en/latest/Tutorials/Mixed?source=Legacy#SomeHeading")]
    [InlineData("/en/about/INTRO.HTML?source=Legacy#SomeHeading", "/en/About/Intro?source=Legacy#SomeHeading")]
    public async Task NavMenu_VersionSwitchResolvesAliasesAndPreservesQueryAndFragment(string source, string expected)
    {
        foreach (var version in new[] { "26.1", "latest" })
        {
            Directory.CreateDirectory(Path.Combine(_root, "docs", version, "Tutorials"));
            File.WriteAllText(Path.Combine(_root, "docs", version, "Tutorials", "Mixed.md"), "# Mixed");
        }
        Directory.CreateDirectory(Path.Combine(_root, "docs", "About"));
        File.WriteAllText(Path.Combine(_root, "docs", "About", "Intro.md"), "# Intro");
        await using var services = CreateServices();
        var navigation = services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(source);
        await using var renderer = new TestRenderer(services);
        int id = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderAsync(typeof(NavMenu), ParameterView.Empty));
        Assert.Contains("doc-current", await renderer.Dispatcher.InvokeAsync(() => renderer.CssClasses(id).ToArray()));
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var select = renderer.FindComponent<MudBlazor.MudSelect<string>>(id);
            Assert.NotNull(select);
            await select.ValueChanged.InvokeAsync("latest");
        });
        Assert.Equal("http://localhost" + expected, navigation.Uri);
        Assert.Empty(renderer.Errors);
    }

    [Fact]
    public async Task Manual_IncludesCcAttributionAndModificationNotice()
    {
        await using var services = CreateServices();
        File.WriteAllText(Path.Combine(_root, "docs", "26.1", "example.md"), "# Example");
        await using var renderer = new TestRenderer(services);
        int id = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderAsync(typeof(DocViewer),
            ParameterView.FromDictionary(new Dictionary<string, object?> { ["DocumentPath"] = "26.1/example" })));

        string text = await renderer.Dispatcher.InvokeAsync(() => renderer.Text(id));
        Assert.Contains("CC BY 3.0", text);
        Assert.Contains("Juan Linietsky, Ariel Manzur and the Godot community", text);
        Assert.Contains("modified from", text);
        Assert.Contains("Sphinx/reStructuredText", text);
        Assert.DoesNotContain("MIT license", text);
        Assert.Empty(renderer.Errors);
    }

    [Theory]
    [InlineData(typeof(NavMenu), null)]
    [InlineData(typeof(ClassDocViewer), null)]
    [InlineData(typeof(ClassDocViewer), "Node")]
    public async Task CatalogPublication_RefreshesExistingComponentsAndStopsAfterDisposal(Type componentType, string? className)
    {
        await using var services = CreateServices();
        var catalog = services.GetRequiredService<ClassDocumentationCatalog>();
        var manager = services.GetRequiredService<VersionManagerService>();
        await using var renderer = new TestRenderer(services);
        var parameters = componentType == typeof(ClassDocViewer)
            ? ParameterView.FromDictionary(new Dictionary<string, object?> { ["VersionSlug"] = "26.1", ["ClassName"] = className })
            : ParameterView.Empty;
        int id = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderAsync(componentType, parameters));
        Assert.DoesNotContain("Node", await renderer.Dispatcher.InvokeAsync(() => renderer.Text(id)));

        ClassDocumentationSnapshot Snapshot(DocumentationVersion version, string description)
            => new(version, description, DateTimeOffset.UtcNow,
                new Dictionary<string, ClassDocumentationEntry>
                {
                    ["Node"] = new() { Name = "Node", BriefDescription = description },
                    [description] = new() { Name = description }
                });

        // Publish off-dispatcher, as the background synchronization service does.
        await Task.Run(() => catalog.Publish(Snapshot(manager.LatestStableVersion, "FirstSnapshot")));
        string first = await renderer.Dispatcher.InvokeAsync(() => renderer.Text(id));
        Assert.Contains("Node", first);
        Assert.Contains("FirstSnapshot", first);

        await Task.Run(() => catalog.Publish(Snapshot(manager.NextPrereleaseVersion, "OtherVersion")));
        Assert.Equal(first, await renderer.Dispatcher.InvokeAsync(() => renderer.Text(id)));

        await Task.Run(() => catalog.Publish(Snapshot(manager.LatestStableVersion, "ReplacementSnapshot")));
        string replacement = await renderer.Dispatcher.InvokeAsync(() => renderer.Text(id));
        Assert.Contains("ReplacementSnapshot", replacement);
        Assert.DoesNotContain("FirstSnapshot", replacement);

        await renderer.Dispatcher.InvokeAsync(() => renderer.Remove(id));
        int batches = renderer.BatchCount;
        await Task.Run(() => catalog.Publish(Snapshot(manager.LatestStableVersion, "AfterDisposal")));
        await renderer.Dispatcher.InvokeAsync(() => Assert.Equal(batches, renderer.BatchCount));
        Assert.Empty(renderer.Errors);
    }

    private ServiceProvider CreateServices(IDocumentationSearch? search = null)
    {
        DocumentationVersion[] versions =
        [
            new() { Slug = "latest", FriendlyName = "Latest", BranchName = "master", IsNextPrerelease = true },
            new() { Slug = "26.1", FriendlyName = "Stable", BranchName = "26.1", IsLatestStable = true }
        ];
        foreach (var version in versions)
            Directory.CreateDirectory(Path.Combine(_root, "docs", version.Slug));
        File.WriteAllText(Path.Combine(_root, "docs", "Versions.json"), JsonSerializer.Serialize(versions));
        var manager = new VersionManagerService(new TestEnvironment(_root));
        manager.LoadContent();
        return new ServiceCollection().AddLogging()
            .AddSingleton<IDocumentationSearch>(search ?? new ControlledSearch())
            .AddMudServices(options => options.PopoverOptions.CheckForPopoverProvider = false)
            .AddSingleton(_ => new DocumentPathResolver(new TestEnvironment(_root), manager))
            .AddSingleton<DocRendererService>()
            .AddSingleton(manager)
            .AddSingleton<ClassDocumentationCatalog>()
            .AddSingleton<ClassDocumentationRenderer>()
            .AddSingleton<NavigationManager, TestNavigationManager>()
            .AddSingleton<IJSRuntime, TestJsRuntime>()
            .BuildServiceProvider();
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private sealed class TestRenderer(IServiceProvider services)
        : Renderer(services, services.GetRequiredService<ILoggerFactory>())
    {
        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
        public List<Exception> Errors { get; } = [];
        public int BatchCount { get; private set; }

        public async Task<int> RenderAsync(Type type, ParameterView parameters)
        {
            int id = AssignRootComponentId(InstantiateComponent(type));
            await RenderRootComponentAsync(id, parameters);
            return id;
        }

        public Task UpdateAsync(int id, ParameterView parameters) => RenderRootComponentAsync(id, parameters);

        public void Remove(int id) => RemoveRootComponent(id);

        public IEnumerable<string> Hrefs(int id)
        {
            var frames = GetCurrentRenderTreeFrames(id);
            foreach (var frame in frames.Array.Take(frames.Count))
            {
                if (frame.FrameType == RenderTreeFrameType.Attribute && frame.AttributeName == "href" && frame.AttributeValue is string href)
                    yield return href;
                if (frame.FrameType == RenderTreeFrameType.Component)
                    foreach (string childHref in Hrefs(frame.ComponentId)) yield return childHref;
            }
        }

        public IEnumerable<string> CssClasses(int id)
        {
            var frames = GetCurrentRenderTreeFrames(id);
            foreach (var frame in frames.Array.Take(frames.Count))
            {
                if (frame.FrameType == RenderTreeFrameType.Attribute && frame.AttributeName == "class" && frame.AttributeValue is string css)
                    foreach (var name in css.Split(' ', StringSplitOptions.RemoveEmptyEntries)) yield return name;
                if (frame.FrameType == RenderTreeFrameType.Component)
                    foreach (var name in CssClasses(frame.ComponentId)) yield return name;
            }
        }

        public T? FindComponent<T>(int id) where T : class
        {
            var frames = GetCurrentRenderTreeFrames(id);
            foreach (var frame in frames.Array.Take(frames.Count).Where(f => f.FrameType == RenderTreeFrameType.Component))
            {
                if (frame.Component is T component) return component;
                if (FindComponent<T>(frame.ComponentId) is T child) return child;
            }
            return null;
        }

        public string Text(int id)
        {
            var frames = GetCurrentRenderTreeFrames(id);
            return string.Concat(frames.Array.Take(frames.Count).Select(frame => frame.FrameType switch
            {
                RenderTreeFrameType.Text => frame.TextContent,
                RenderTreeFrameType.Markup => frame.MarkupContent,
                RenderTreeFrameType.Component => Text(frame.ComponentId),
                _ => string.Empty
            }));
        }

        protected override IComponent ResolveComponentForRenderMode(Type type, int? parentId,
            IComponentActivator activator, IComponentRenderMode mode) => activator.CreateInstance(type);
        protected override void HandleException(Exception exception) => Errors.Add(exception);
        protected override Task UpdateDisplayAsync(in RenderBatch batch)
        {
            BatchCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager() => Initialize("http://localhost/", "http://localhost/en/26.1/Classes");
        protected override void NavigateToCore(string uri, NavigationOptions options) => NavigateToCore(uri, options.ForceLoad);
        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            Uri = ToAbsoluteUri(uri).AbsoluteUri;
            NotifyLocationChanged(false);
        }
    }

    private sealed class TestJsRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult(default(TValue)!);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => InvokeAsync<TValue>(identifier, args);
    }

    private sealed class TestEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Redot_Documentation_Tests";
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = root;
        public string WebRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
#pragma warning restore BL0006
