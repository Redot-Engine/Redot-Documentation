using Redot_Documentation.Versioning;

namespace Redot_Documentation.ClassDocumentation;

/// <summary>Represents an immutable class-documentation snapshot for one version.</summary>
/// <param name="Version">The documentation version.</param>
/// <param name="CommitSha">The source commit identifier.</param>
/// <param name="SynchronizedAt">The snapshot synchronization time.</param>
/// <param name="Classes">The classes keyed by name.</param>
public sealed record ClassDocumentationSnapshot(
    DocumentationVersion Version,
    string CommitSha,
    DateTimeOffset SynchronizedAt,
    IReadOnlyDictionary<string, ClassDocumentationEntry> Classes)
{
    /// <summary>Finds a class by name.</summary>
    /// <param name="name">The class name.</param>
    /// <param name="classDocumentation">The matching class, when found.</param>
    /// <returns><see langword="true"/> when the class exists; otherwise, <see langword="false"/>.</returns>
    public bool TryGetClass(string name, out ClassDocumentationEntry? classDocumentation)
        => Classes.TryGetValue(name, out classDocumentation);
}

/// <summary>Describes one documented engine class.</summary>
public sealed record ClassDocumentationEntry
{
    /// <summary>Gets the class name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the inherited class name.</summary>
    public string? Inherits { get; init; }

    /// <summary>Gets the brief description.</summary>
    public string BriefDescription { get; init; } = string.Empty;

    /// <summary>Gets the full description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the deprecation message.</summary>
    public string? Deprecated { get; init; }

    /// <summary>Gets the experimental-status message.</summary>
    public string? Experimental { get; init; }

    /// <summary>Gets the constructors.</summary>
    public IReadOnlyList<ClassDocumentationCallable> Constructors { get; init; } = [];

    /// <summary>Gets the methods.</summary>
    public IReadOnlyList<ClassDocumentationCallable> Methods { get; init; } = [];

    /// <summary>Gets the properties.</summary>
    public IReadOnlyList<ClassDocumentationMember> Members { get; init; } = [];

    /// <summary>Gets the signals.</summary>
    public IReadOnlyList<ClassDocumentationSignal> Signals { get; init; } = [];

    /// <summary>Gets the constants.</summary>
    public IReadOnlyList<ClassDocumentationConstant> Constants { get; init; } = [];

    /// <summary>Gets the operators.</summary>
    public IReadOnlyList<ClassDocumentationCallable> Operators { get; init; } = [];

    /// <summary>Gets the annotations.</summary>
    public IReadOnlyList<ClassDocumentationCallable> Annotations { get; init; } = [];

    /// <summary>Gets the theme items.</summary>
    public IReadOnlyList<ClassDocumentationThemeItem> ThemeItems { get; init; } = [];

    /// <summary>Gets the tutorials.</summary>
    public IReadOnlyList<ClassDocumentationTutorial> Tutorials { get; init; } = [];
}

/// <summary>Describes a callable class API member.</summary>
public sealed record ClassDocumentationCallable
{
    /// <summary>Gets the callable name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the return type.</summary>
    public string ReturnType { get; init; } = "void";

    /// <summary>Gets the return enum name.</summary>
    public string? ReturnEnum { get; init; }

    /// <summary>Gets the callable qualifiers.</summary>
    public string? Qualifiers { get; init; }

    /// <summary>Gets the description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the deprecation message.</summary>
    public string? Deprecated { get; init; }

    /// <summary>Gets the experimental-status message.</summary>
    public string? Experimental { get; init; }

    /// <summary>Gets the parameters.</summary>
    public IReadOnlyList<ClassDocumentationParameter> Parameters { get; init; } = [];
}

/// <summary>Describes a callable parameter.</summary>
/// <param name="Name">The parameter name.</param>
/// <param name="Type">The parameter type.</param>
/// <param name="Default">The default value.</param>
/// <param name="Enum">The enum name.</param>
/// <param name="IsBitField">Whether the parameter is a bit field.</param>
public sealed record ClassDocumentationParameter(
    string Name,
    string Type,
    string? Default,
    string? Enum,
    bool IsBitField);

/// <summary>Describes a documented class property.</summary>
public sealed record ClassDocumentationMember
{
    /// <summary>Gets the property name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the property type.</summary>
    public required string Type { get; init; }

    /// <summary>Gets the default value.</summary>
    public string? Default { get; init; }

    /// <summary>Gets the setter name.</summary>
    public string? Setter { get; init; }

    /// <summary>Gets the getter name.</summary>
    public string? Getter { get; init; }

    /// <summary>Gets the enum name.</summary>
    public string? Enum { get; init; }

    /// <summary>Gets whether the property is a bit field.</summary>
    public bool IsBitField { get; init; }

    /// <summary>Gets the description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the deprecation message.</summary>
    public string? Deprecated { get; init; }

    /// <summary>Gets the experimental-status message.</summary>
    public string? Experimental { get; init; }
}

/// <summary>Describes a documented signal.</summary>
public sealed record ClassDocumentationSignal
{
    /// <summary>Gets the signal name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the deprecation message.</summary>
    public string? Deprecated { get; init; }

    /// <summary>Gets the experimental-status message.</summary>
    public string? Experimental { get; init; }

    /// <summary>Gets the signal parameters.</summary>
    public IReadOnlyList<ClassDocumentationParameter> Parameters { get; init; } = [];
}

/// <summary>Describes a documented class constant.</summary>
public sealed record ClassDocumentationConstant
{
    /// <summary>Gets the constant name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the constant value.</summary>
    public required string Value { get; init; }

    /// <summary>Gets the enum name.</summary>
    public string? Enum { get; init; }

    /// <summary>Gets whether the constant is a bit field.</summary>
    public bool IsBitField { get; init; }

    /// <summary>Gets the description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the deprecation message.</summary>
    public string? Deprecated { get; init; }

    /// <summary>Gets the experimental-status message.</summary>
    public string? Experimental { get; init; }
}

/// <summary>Describes a documented theme item.</summary>
public sealed record ClassDocumentationThemeItem
{
    /// <summary>Gets the theme-item name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the theme-item type.</summary>
    public required string Type { get; init; }

    /// <summary>Gets the underlying data type.</summary>
    public string? DataType { get; init; }

    /// <summary>Gets the default value.</summary>
    public string? Default { get; init; }

    /// <summary>Gets the description.</summary>
    public string Description { get; init; } = string.Empty;
}

/// <summary>Describes a class tutorial link.</summary>
/// <param name="Title">The link title.</param>
/// <param name="Url">The link URL.</param>
public sealed record ClassDocumentationTutorial(string Title, string Url);
