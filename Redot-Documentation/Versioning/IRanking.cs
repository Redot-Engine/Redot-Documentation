using System.Text;

namespace Redot_Documentation.Versioning;

public interface IRanking : IComparable<IRanking>
{
    public string Name { get; set; }
    public int Rank { get; set; }
    public string Path { get; set; }
    public string Slug { get; set; }

    private static HashSet<string> _notCapitalizedWords = new HashSet<string>
    {
        "a",
        "an",
        "and",
        "as",
        "at",
        "but,",
        "by",
        "for",
        "from",
        "if",
        "in",
        "into",
        "near",
        "nor",
        "of",
        "off",
        "on",
        "once",
        "onto",
        "or",
        "over",
        "past",
        "per",
        "so",
        "than",
        "that",
        "the",
        "to",
        "when",
        "with",
        "yet"

    };

    private static Dictionary<string, string> _specialWords = new Dictionary<string, string>()
    {
        {"faq", "FAQ"},
        {"ip", "IP"},
        {"mac", "MAC"}
    };

    public string GetDisplayName()
    {
        string temp = Name.Replace('_', ' ');
        int extensionLoc = temp.LastIndexOf('.');
        if (extensionLoc > -1)
        {
            temp = temp.Substring(0, extensionLoc);
        }
        temp = temp.Trim();
        StringBuilder builder = new StringBuilder(temp.Length);
        string[] words = temp.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            void AddSpace()
            {
                if (i < words.Length - 1)
                {
                    builder.Append(" ");
                }
            }
            string lowerVersion = words[i].ToLowerInvariant();
            if (i > 0 && _notCapitalizedWords.Contains(lowerVersion))
            {
                builder.Append(words[i].ToLower());
                AddSpace();
                continue;
            }
            if (_specialWords.TryGetValue(lowerVersion, out string? specialWord))
            {
                builder.Append(specialWord);
                AddSpace();
                continue;
            }
            for (int j = 0; j < words[i].Length; j++)
            {
                if (j == 0)
                {
                    builder.Append(words[i][j].ToString().ToUpper());
                }
                else
                {
                    builder.Append(words[i][j].ToString());
                }
            }
            AddSpace();
        }
        return builder.ToString();
    }
}