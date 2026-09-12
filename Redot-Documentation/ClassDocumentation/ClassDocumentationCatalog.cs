using System.Collections.ObjectModel;

namespace Redot_Documentation.ClassDocumentation;

/// <summary>Stores the active class-documentation snapshots.</summary>
public sealed class ClassDocumentationCatalog
{
    /// <summary>Notifies subscribers after a version snapshot is published.</summary>
    public event Action<string>? Changed;

    /// <summary>Synchronizes snapshot publication.</summary>
    private readonly object _gate = new();

    /// <summary>Contains the published snapshots keyed by version slug.</summary>
    private IReadOnlyDictionary<string, ClassDocumentationSnapshot> _snapshots =
        new ReadOnlyDictionary<string, ClassDocumentationSnapshot>(
            new Dictionary<string, ClassDocumentationSnapshot>(StringComparer.OrdinalIgnoreCase));

    /// <summary>Publishes a snapshot atomically.</summary>
    /// <param name="snapshot">The snapshot to publish.</param>
    public void Publish(ClassDocumentationSnapshot snapshot)
    {
        lock (_gate)
        {
            var updated = new Dictionary<string, ClassDocumentationSnapshot>(_snapshots, StringComparer.OrdinalIgnoreCase)
            {
                [snapshot.Version.Slug] = snapshot
            };
            Volatile.Write(ref _snapshots, new ReadOnlyDictionary<string, ClassDocumentationSnapshot>(updated));
        }

        Changed?.Invoke(snapshot.Version.Slug);
    }

    /// <summary>Finds a snapshot by version slug.</summary>
    /// <param name="versionSlug">The version slug.</param>
    /// <param name="snapshot">The matching snapshot, when found.</param>
    /// <returns><see langword="true"/> when the snapshot exists; otherwise, <see langword="false"/>.</returns>
    public bool TryGetSnapshot(string versionSlug, out ClassDocumentationSnapshot? snapshot)
        => Volatile.Read(ref _snapshots).TryGetValue(versionSlug, out snapshot);

    /// <summary>Finds a class in a version snapshot.</summary>
    /// <param name="versionSlug">The version slug.</param>
    /// <param name="className">The class name.</param>
    /// <param name="classDocumentation">The matching class, when found.</param>
    /// <returns><see langword="true"/> when the class exists; otherwise, <see langword="false"/>.</returns>
    public bool TryGetClass(
        string versionSlug,
        string className,
        out ClassDocumentationEntry? classDocumentation)
    {
        classDocumentation = null;
        return TryGetSnapshot(versionSlug, out ClassDocumentationSnapshot? snapshot)
            && snapshot!.TryGetClass(className, out classDocumentation);
    }

    /// <summary>Gets the classes for a version ordered by name.</summary>
    /// <param name="versionSlug">The version slug.</param>
    /// <returns>The ordered classes, or an empty list when unavailable.</returns>
    public IReadOnlyList<ClassDocumentationEntry> GetClassesAlphabetically(string versionSlug)
        => TryGetSnapshot(versionSlug, out ClassDocumentationSnapshot? snapshot)
            ? snapshot!.Classes.Values
                .OrderBy(classDocumentation => classDocumentation.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : [];
}
