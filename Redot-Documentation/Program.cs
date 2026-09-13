
using Redot_Documentation.Components;
using MudBlazor.Services;
using Redot_Documentation.ClassDocumentation;
using Redot_Documentation.Services;

namespace Redot_Documentation;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
        builder.Services.AddMudServices();
        builder.Services.AddScoped<DocRendererService>();
        builder.Services.AddSingleton<VersionManagerService>();
        builder.Services.Configure<ClassDocumentationOptions>(
            builder.Configuration.GetSection(ClassDocumentationOptions.SectionName));
        builder.Services.AddSingleton<ClassDocumentationCatalog>();
        builder.Services.AddSingleton<ClassDocumentationParser>();
        builder.Services.AddSingleton<ClassDocumentationRenderer>();
        builder.Services.AddSingleton<IGitCommandRunner, GitCommandRunner>();
        builder.Services.AddSingleton<IClassDocumentationSource, GitClassDocumentationSource>();
        builder.Services.AddHostedService<ClassDocumentationSyncService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);
        app.Services.GetService<VersionManagerService>()?.LoadContent();
        app.MapGet("/health/class-docs", (
            ClassDocumentationCatalog catalog,
            VersionManagerService versionManager) =>
        {
            var versions = versionManager.Versions.Select(version =>
            {
                bool available = catalog.TryGetSnapshot(version.Slug, out ClassDocumentationSnapshot? snapshot);
                return new
                {
                    version = version.Slug,
                    branch = version.BranchName,
                    available,
                    commit = snapshot?.CommitSha,
                    synchronizedAt = snapshot?.SynchronizedAt,
                    classCount = snapshot?.Classes.Count ?? 0
                };
            }).ToArray();

            return versions.All(version => version.available)
                ? Results.Ok(versions)
                : Results.Json(versions, statusCode: StatusCodes.Status503ServiceUnavailable);
        });
        app.Run();
    }
}
