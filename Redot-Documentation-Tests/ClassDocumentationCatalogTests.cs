using Redot_Documentation.ClassDocumentation;
using Redot_Documentation.Versioning;

namespace Redot_Documentation_Tests;

public sealed class ClassDocumentationCatalogTests
{
    [Fact]
    public void GetClassesAlphabetically_ReturnsOnlyTheRequestedVersionInNameOrder()
    {
        var catalog = new ClassDocumentationCatalog();
        catalog.Publish(CreateSnapshot("latest", "Zoo", "alpha", "Beta"));
        catalog.Publish(CreateSnapshot("26.1", "Node"));

        IReadOnlyList<ClassDocumentationEntry> classes = catalog.GetClassesAlphabetically("LATEST");

        Assert.Equal(["alpha", "Beta", "Zoo"], classes.Select(entry => entry.Name));
    }

    [Fact]
    public void GetClassesAlphabetically_ReturnsAnEmptyListForAnUnavailableVersion()
    {
        var catalog = new ClassDocumentationCatalog();

        Assert.Empty(catalog.GetClassesAlphabetically("missing"));
    }

    private static ClassDocumentationSnapshot CreateSnapshot(string slug, params string[] names)
    {
        var version = new DocumentationVersion
        {
            Slug = slug,
            FriendlyName = slug,
            BranchName = slug
        };
        var classes = names.ToDictionary(
            name => name,
            name => new ClassDocumentationEntry { Name = name },
            StringComparer.OrdinalIgnoreCase);
        return new ClassDocumentationSnapshot(version, "abc123", DateTimeOffset.UtcNow, classes);
    }
}
