namespace Redot_Documentation.ClassDocumentation;

/// <summary>Configures class-documentation synchronization.</summary>
public sealed class ClassDocumentationOptions
{
    /// <summary>Defines the configuration section name.</summary>
    public const string SectionName = "ClassDocumentation";

    /// <summary>Gets or sets whether synchronization is enabled.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Gets or sets the source repository URL.</summary>
    public string RepositoryUrl { get; set; } = "https://github.com/Redot-Engine/redot-engine.git";

    /// <summary>Gets or sets the repository-relative class-documentation path.</summary>
    public string RepositoryPath { get; set; } = "doc/classes";

    /// <summary>Gets or sets the local cache path.</summary>
    public string CacheRoot { get; set; } = "App_Data/class-docs";

    /// <summary>Gets or sets the interval between synchronizations.</summary>
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Gets or sets the timeout for each Git command.</summary>
    public TimeSpan GitTimeout { get; set; } = TimeSpan.FromMinutes(5);
}
