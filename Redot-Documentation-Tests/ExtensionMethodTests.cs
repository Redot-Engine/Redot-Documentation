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
}