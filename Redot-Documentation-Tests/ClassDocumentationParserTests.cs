using Redot_Documentation.ClassDocumentation;

namespace Redot_Documentation_Tests;

public sealed class ClassDocumentationParserTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        $"redot-class-parser-tests-{Guid.NewGuid():N}");

    [Fact]
    public void ParseDirectory_ParsesTheSupportedClassSchema()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, "Node.xml"), """
            <?xml version="1.0" encoding="UTF-8" ?>
            <class name="Node" inherits="Object" experimental="Still evolving.">
                <brief_description>A base [b]node[/b].</brief_description>
                <description>Long description.</description>
                <tutorials><link title="Nodes">https://example.com/nodes</link></tutorials>
                <constructors>
                    <constructor name="Node"><return type="Node"/><description>Creates one.</description></constructor>
                </constructors>
                <methods>
                    <method name="add_child" qualifiers="const">
                        <return type="void"/>
                        <param index="0" name="child" type="Node"/>
                        <param index="1" name="force" type="bool" default="false"/>
                        <description>Adds [param child].</description>
                    </method>
                </methods>
                <members>
                    <member name="name" type="StringName" setter="set_name" getter="get_name" default="&amp;&quot;&amp;&quot;">The name.</member>
                </members>
                <signals>
                    <signal name="renamed"><param index="0" name="old_name" type="StringName"/><description>Emitted.</description></signal>
                </signals>
                <constants>
                    <constant name="NOTIFICATION_READY" value="13" enum="Notification">Ready.</constant>
                </constants>
                <operators>
                    <operator name="operator =="><return type="bool"/><param index="0" name="right" type="Node"/><description>Compares.</description></operator>
                </operators>
                <annotations>
                    <annotation name="@rpc"><return type="void"/><description>RPC configuration.</description></annotation>
                </annotations>
                <theme_items>
                    <theme_item name="font" data_type="font" type="Font">The font.</theme_item>
                </theme_items>
            </class>
            """);

        var parser = new ClassDocumentationParser();

        IReadOnlyDictionary<string, ClassDocumentationEntry> classes = parser.ParseDirectory(_directory);

        ClassDocumentationEntry node = Assert.Single(classes).Value;
        Assert.Equal("Node", node.Name);
        Assert.Equal("Object", node.Inherits);
        Assert.Equal("Still evolving.", node.Experimental);
        Assert.Equal("A base [b]node[/b].", node.BriefDescription);
        Assert.Equal("Node", Assert.Single(node.Constructors).ReturnType);
        ClassDocumentationCallable method = Assert.Single(node.Methods);
        Assert.Equal("add_child", method.Name);
        Assert.Equal("const", method.Qualifiers);
        Assert.Equal("false", method.Parameters[1].Default);
        Assert.Equal("The name.", Assert.Single(node.Members).Description);
        Assert.Equal("old_name", Assert.Single(node.Signals).Parameters[0].Name);
        Assert.Equal("Notification", Assert.Single(node.Constants).Enum);
        Assert.Equal("operator ==", Assert.Single(node.Operators).Name);
        Assert.Equal("@rpc", Assert.Single(node.Annotations).Name);
        Assert.Equal("font", Assert.Single(node.ThemeItems).DataType);
        Assert.Equal("https://example.com/nodes", Assert.Single(node.Tutorials).Url);
    }

    [Fact]
    public void ParseFile_RejectsDocumentTypeDeclarations()
    {
        Directory.CreateDirectory(_directory);
        string filePath = Path.Combine(_directory, "Unsafe.xml");
        File.WriteAllText(filePath, """
            <!DOCTYPE class [<!ENTITY payload SYSTEM "file:///etc/passwd">]>
            <class name="Unsafe"><brief_description>&payload;</brief_description></class>
            """);

        var parser = new ClassDocumentationParser();

        InvalidDataException exception = Assert.Throws<InvalidDataException>(() => parser.ParseFile(filePath));
        Assert.IsType<System.Xml.XmlException>(exception.InnerException);
    }

    [Theory]
    [InlineData("<class xmlns=\"urn:redot\" name=\"Node\"><description>Lost text</description></class>")]
    [InlineData("<api:class xmlns:api=\"urn:redot\" name=\"Node\" />")]
    [InlineData("<class name=\"Node\"><constants><constant name=\"READY\" /></constants></class>")]
    [InlineData("<class name=\"Node\"><constants><constant name=\"READY\" value=\"\" /></constants></class>")]
    [InlineData("<class name=\"Node\"><constants><constant name=\"READY\" value=\"  \" /></constants></class>")]
    public void ParseDirectory_RejectsIncompleteClassData(string xml)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, "Node.xml"), xml);

        Assert.Throws<InvalidDataException>(() => new ClassDocumentationParser().ParseDirectory(_directory));
    }

    [Fact]
    public void ParseDirectory_AcceptsUnnamespacedClassAndZeroConstant()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, "Node.xml"),
            "<class xmlns=\"\" name=\"Node\"><constants><constant name=\"ZERO\" value=\"0\" /></constants></class>");

        ClassDocumentationEntry entry = Assert.Single(new ClassDocumentationParser().ParseDirectory(_directory)).Value;

        Assert.Equal("0", Assert.Single(entry.Constants).Value);
    }

    [Fact]
    public void ParseFile_PreservesEmptyStatusAttributes()
    {
        Directory.CreateDirectory(_directory);
        string filePath = Path.Combine(_directory, "Legacy.xml");
        File.WriteAllText(filePath, "<class name=\"Legacy\" deprecated=\"\"><brief_description /></class>");

        ClassDocumentationEntry entry = new ClassDocumentationParser().ParseFile(filePath);

        Assert.NotNull(entry.Deprecated);
        Assert.Equal(string.Empty, entry.Deprecated);
    }

    [Fact]
    public void ParseDirectories_ReportsBothFilesForDuplicateClasses()
    {
        string core = Path.Combine(_directory, "core");
        string module = Path.Combine(_directory, "module");
        Directory.CreateDirectory(core);
        Directory.CreateDirectory(module);
        string first = Path.Combine(core, "Node.xml");
        string second = Path.Combine(module, "Node.xml");
        File.WriteAllText(first, "<class name=\"Node\" />");
        File.WriteAllText(second, "<class name=\"node\" />");
        var error = Assert.Throws<InvalidDataException>(() => new ClassDocumentationParser().ParseDirectories([core, module]));
        Assert.Contains(first, error.Message);
        Assert.Contains(second, error.Message);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }
}
