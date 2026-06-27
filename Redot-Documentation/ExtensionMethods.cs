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

    /// <summary>
    /// Determines whether the input string starts with any of the specified prefixes.
    /// </summary>
    /// <param name="input">The input string to check for matching prefixes.</param>
    /// <param name="prefixes">A collection of prefix strings to compare against the beginning of the input string.</param>
    /// <returns>True if the input string starts with any of the provided prefixes; otherwise, false.</returns>
    public static bool StartsWithAny(this string input, IEnumerable<string> prefixes)
    {
        foreach (var pfx in prefixes)
        {
            if (input.StartsWith(pfx))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Determines if the input string starts with any of the specified prefixes.
    /// </summary>
    /// <param name="input">The input string to check.</param>
    /// <param name="prefixes">An array of strings representing the prefixes to test against.</param>
    /// <returns>True if the input string starts with any of the specified prefixes, otherwise false.</returns>
    public static bool StartsWithAny(this string input, params string[] prefixes)
    {
        return input.StartsWithAny((IEnumerable<string>)prefixes);
    }
}