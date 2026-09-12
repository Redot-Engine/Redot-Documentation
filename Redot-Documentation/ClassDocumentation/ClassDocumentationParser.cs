using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;

namespace Redot_Documentation.ClassDocumentation;

/// <summary>Parses Redot class-reference XML files.</summary>
public sealed class ClassDocumentationParser
{
    /// <summary>Parses every class XML file in a directory.</summary>
    /// <param name="classDocumentationPath">The directory containing class XML files.</param>
    /// <returns>The parsed classes keyed by name.</returns>
    /// <exception cref="DirectoryNotFoundException">The directory does not exist.</exception>
    /// <exception cref="InvalidDataException">The directory is empty or contains invalid or duplicate classes.</exception>
    /// <exception cref="IOException">A class file cannot be read.</exception>
    /// <exception cref="UnauthorizedAccessException">A class file cannot be accessed.</exception>
    public IReadOnlyDictionary<string, ClassDocumentationEntry> ParseDirectory(string classDocumentationPath)
    {
        if (!Directory.Exists(classDocumentationPath))
            throw new DirectoryNotFoundException($"Class documentation directory was not found: {classDocumentationPath}");

        var classes = new Dictionary<string, ClassDocumentationEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (string filePath in Directory.EnumerateFiles(classDocumentationPath, "*.xml", SearchOption.TopDirectoryOnly))
        {
            ClassDocumentationEntry entry = ParseFile(filePath);
            if (!classes.TryAdd(entry.Name, entry))
                throw new InvalidDataException($"Duplicate class documentation entry '{entry.Name}'.");
        }

        if (classes.Count == 0)
            throw new InvalidDataException($"No class XML files were found in '{classDocumentationPath}'.");

        return new ReadOnlyDictionary<string, ClassDocumentationEntry>(classes);
    }

    /// <summary>Parses one class XML file.</summary>
    /// <param name="filePath">The XML file path.</param>
    /// <returns>The parsed class.</returns>
    /// <exception cref="InvalidDataException">The XML or required class data is invalid.</exception>
    /// <exception cref="IOException">The file cannot be read.</exception>
    /// <exception cref="UnauthorizedAccessException">The file cannot be accessed.</exception>
    public ClassDocumentationEntry ParseFile(string filePath)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = 8 * 1024 * 1024,
            IgnoreComments = true
        };

        try
        {
            using XmlReader reader = XmlReader.Create(filePath, settings);
            XDocument document = XDocument.Load(reader, LoadOptions.None);
            XElement root = document.Root
                ?? throw new InvalidDataException($"Class XML '{filePath}' has no root element.");

            if (root.Name != XName.Get("class"))
                throw new InvalidDataException($"Class XML '{filePath}' must have an unnamespaced class root element.");

            string name = RequiredAttribute(root, "name", filePath);
            return new ClassDocumentationEntry
            {
                Name = name,
                Inherits = Attribute(root, "inherits"),
                BriefDescription = ElementText(root, "brief_description"),
                Description = ElementText(root, "description"),
                Deprecated = OptionalMessage(root, "deprecated"),
                Experimental = OptionalMessage(root, "experimental"),
                Constructors = ParseCallables(root.Element("constructors"), "constructor"),
                Methods = ParseCallables(root.Element("methods"), "method"),
                Members = ParseMembers(root.Element("members")),
                Signals = ParseSignals(root.Element("signals")),
                Constants = ParseConstants(root.Element("constants")),
                Operators = ParseCallables(root.Element("operators"), "operator"),
                Annotations = ParseCallables(root.Element("annotations"), "annotation"),
                ThemeItems = ParseThemeItems(root.Element("theme_items")),
                Tutorials = ParseTutorials(root.Element("tutorials"))
            };
        }
        catch (XmlException exception)
        {
            throw new InvalidDataException($"Class XML '{filePath}' is invalid.", exception);
        }
    }

    /// <summary>Parses callable elements from a container.</summary>
    /// <param name="container">The optional callable container.</param>
    /// <param name="elementName">The callable element name.</param>
    /// <returns>The parsed callables.</returns>
    /// <exception cref="InvalidDataException">A callable lacks required data.</exception>
    private static IReadOnlyList<ClassDocumentationCallable> ParseCallables(XElement? container, string elementName)
        => container?.Elements(elementName).Select(element =>
        {
            XElement? returnElement = element.Element("return");
            return new ClassDocumentationCallable
            {
                Name = RequiredAttribute(element, "name", elementName),
                ReturnType = Attribute(returnElement, "type") ?? "void",
                ReturnEnum = Attribute(returnElement, "enum"),
                Qualifiers = Attribute(element, "qualifiers"),
                Description = ElementText(element, "description"),
                Deprecated = OptionalMessage(element, "deprecated"),
                Experimental = OptionalMessage(element, "experimental"),
                Parameters = element.Elements()
                    .Where(child => child.Name.LocalName is "param" or "argument")
                    .Select(ParseParameter)
                    .ToArray()
            };
        }).ToArray() ?? [];

    /// <summary>Parses a callable parameter.</summary>
    /// <param name="element">The parameter element.</param>
    /// <returns>The parsed parameter.</returns>
    /// <exception cref="InvalidDataException">The parameter lacks a name.</exception>
    private static ClassDocumentationParameter ParseParameter(XElement element)
        => new(
            RequiredAttribute(element, "name", "parameter"),
            Attribute(element, "type") ?? "Variant",
            Attribute(element, "default"),
            Attribute(element, "enum"),
            ParseBooleanAttribute(element, "is_bitfield"));

    /// <summary>Parses class properties from a container.</summary>
    /// <param name="container">The optional property container.</param>
    /// <returns>The parsed properties.</returns>
    /// <exception cref="InvalidDataException">A property lacks required data.</exception>
    private static IReadOnlyList<ClassDocumentationMember> ParseMembers(XElement? container)
        => container?.Elements("member").Select(element => new ClassDocumentationMember
        {
            Name = RequiredAttribute(element, "name", "member"),
            Type = Attribute(element, "type") ?? "Variant",
            Default = Attribute(element, "default"),
            Setter = Attribute(element, "setter"),
            Getter = Attribute(element, "getter"),
            Enum = Attribute(element, "enum"),
            IsBitField = ParseBooleanAttribute(element, "is_bitfield"),
            Description = DescriptionText(element),
            Deprecated = OptionalMessage(element, "deprecated"),
            Experimental = OptionalMessage(element, "experimental")
        }).ToArray() ?? [];

    /// <summary>Parses signals from a container.</summary>
    /// <param name="container">The optional signal container.</param>
    /// <returns>The parsed signals.</returns>
    /// <exception cref="InvalidDataException">A signal lacks required data.</exception>
    private static IReadOnlyList<ClassDocumentationSignal> ParseSignals(XElement? container)
        => container?.Elements("signal").Select(element => new ClassDocumentationSignal
        {
            Name = RequiredAttribute(element, "name", "signal"),
            Description = ElementText(element, "description"),
            Deprecated = OptionalMessage(element, "deprecated"),
            Experimental = OptionalMessage(element, "experimental"),
            Parameters = element.Elements()
                .Where(child => child.Name.LocalName is "param" or "argument")
                .Select(ParseParameter)
                .ToArray()
        }).ToArray() ?? [];

    /// <summary>Parses constants from a container.</summary>
    /// <param name="container">The optional constant container.</param>
    /// <returns>The parsed constants.</returns>
    /// <exception cref="InvalidDataException">A constant lacks required data.</exception>
    private static IReadOnlyList<ClassDocumentationConstant> ParseConstants(XElement? container)
        => container?.Elements("constant").Select(element => new ClassDocumentationConstant
        {
            Name = RequiredAttribute(element, "name", "constant"),
            Value = RequiredAttribute(element, "value", "constant"),
            Enum = Attribute(element, "enum"),
            IsBitField = ParseBooleanAttribute(element, "is_bitfield"),
            Description = DescriptionText(element),
            Deprecated = OptionalMessage(element, "deprecated"),
            Experimental = OptionalMessage(element, "experimental")
        }).ToArray() ?? [];

    /// <summary>Parses theme items from a container.</summary>
    /// <param name="container">The optional theme-item container.</param>
    /// <returns>The parsed theme items.</returns>
    /// <exception cref="InvalidDataException">A theme item lacks required data.</exception>
    private static IReadOnlyList<ClassDocumentationThemeItem> ParseThemeItems(XElement? container)
        => container?.Elements("theme_item").Select(element => new ClassDocumentationThemeItem
        {
            Name = RequiredAttribute(element, "name", "theme_item"),
            Type = Attribute(element, "type") ?? "Variant",
            DataType = Attribute(element, "data_type"),
            Default = Attribute(element, "default"),
            Description = DescriptionText(element)
        }).ToArray() ?? [];

    /// <summary>Parses tutorial links from a container.</summary>
    /// <param name="container">The optional tutorial container.</param>
    /// <returns>The parsed tutorial links.</returns>
    private static IReadOnlyList<ClassDocumentationTutorial> ParseTutorials(XElement? container)
        => container?.Elements("link")
            .Select(element => new ClassDocumentationTutorial(
                Attribute(element, "title") ?? element.Value.Trim(),
                element.Value.Trim()))
            .Where(tutorial => !string.IsNullOrWhiteSpace(tutorial.Url))
            .ToArray() ?? [];

    /// <summary>Extracts description text from an element.</summary>
    /// <param name="element">The source element.</param>
    /// <returns>The normalized description.</returns>
    private static string DescriptionText(XElement element)
        => element.Element("description") is XElement description
            ? Normalize(description.Value)
            : Normalize(string.Concat(element.Nodes().OfType<XText>().Select(node => node.Value)));

    /// <summary>Extracts normalized text from a child element.</summary>
    /// <param name="parent">The parent element.</param>
    /// <param name="name">The child element name.</param>
    /// <returns>The normalized child text.</returns>
    private static string ElementText(XElement parent, string name)
        => Normalize(parent.Element(name)?.Value ?? string.Empty);

    /// <summary>Normalizes line endings and surrounding whitespace.</summary>
    /// <param name="value">The text to normalize.</param>
    /// <returns>The normalized text.</returns>
    private static string Normalize(string value)
        => value.Replace("\r\n", "\n", StringComparison.Ordinal).Trim();

    /// <summary>Gets a required attribute value.</summary>
    /// <param name="element">The containing element.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="context">The element description used in errors.</param>
    /// <returns>The nonempty attribute value.</returns>
    /// <exception cref="InvalidDataException">The attribute is absent or empty.</exception>
    private static string RequiredAttribute(XElement element, string name, string context)
        => Attribute(element, name) is { Length: > 0 } value
            ? value
            : throw new InvalidDataException($"A {context} element is missing its required '{name}' attribute.");

    /// <summary>Gets a trimmed optional attribute value.</summary>
    /// <param name="element">The optional containing element.</param>
    /// <param name="name">The attribute name.</param>
    /// <returns>The value, or <see langword="null"/> when absent or empty.</returns>
    private static string? Attribute(XElement? element, string name)
        => element?.Attribute(name)?.Value.Trim() is { Length: > 0 } value ? value : null;

    /// <summary>Gets an optional status message.</summary>
    /// <param name="element">The containing element.</param>
    /// <param name="name">The status attribute name.</param>
    /// <returns>The trimmed message, or <see langword="null"/> when absent.</returns>
    private static string? OptionalMessage(XElement element, string name)
        => element.Attribute(name)?.Value.Trim();

    /// <summary>Parses an optional Boolean attribute.</summary>
    /// <param name="element">The containing element.</param>
    /// <param name="name">The attribute name.</param>
    /// <returns>The parsed value, or <see langword="false"/> when absent or invalid.</returns>
    private static bool ParseBooleanAttribute(XElement element, string name)
        => bool.TryParse(Attribute(element, name), out bool value) && value;
}
