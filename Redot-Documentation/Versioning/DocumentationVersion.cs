namespace Redot_Documentation.Versioning;

public sealed class DocumentationVersion
{
    public required string Slug { get; init; }

    public required string FriendlyName { get; init; }

    public required string BranchName { get; init; }

    public bool IsLatestStable { get; init; }

    public bool IsNextPrerelease { get; init; }
}
