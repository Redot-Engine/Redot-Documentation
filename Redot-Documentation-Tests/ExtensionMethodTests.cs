using Redot_Documentation;

namespace Redot_Documentation_Tests;

public class ExtensionMethodTests
{
    [Theory]
    [InlineData("hello world", "Hello World")]
    [InlineData("this is a test", "This Is A Test")]
    [InlineData("the Quick brown fox jumps over the Lazy dog", "The Quick Brown Fox Jumps Over The Lazy Dog")]
    public void TestCapitalizeEachWord(string input, string expectedOutput)
    {
        input = input.CapitalizeEachWord();
        Assert.Equal(expectedOutput, input);
    }

    [Theory]
    [InlineData("hello world", new string[] { "hello", "world" }, true)]
    [InlineData("this is a test", new string[] { "this", "test" }, true)]
    [InlineData("the Quick brown fox jumps over the Lazy dog", new string[] { "fox", "dog" }, false)]
    public void TestStartsWithAny(string input, string[] prefixes, bool expectedOutput)
    {
        bool result = input.StartsWithAny(prefixes);
        Assert.Equal(expectedOutput, result);
    }
}