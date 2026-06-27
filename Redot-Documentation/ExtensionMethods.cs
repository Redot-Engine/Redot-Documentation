namespace Redot_Documentation;

public static class ExtensionMethods
{
    /// <summary>
    /// Capitalizes the first letter of each word in the input string.
    /// </summary>
    /// <param name="input">The input string in which each word's first letter will be capitalized.</param>
    /// <returns>A new string with each word's first letter capitalized.</returns>
    public static string CapitalizeEachWord(this string input)
    {
        return string.Join(" ", input.Split(' ').Select(word => char.ToUpper(word[0]) + word.Substring(1)));
    }
}