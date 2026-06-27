using System.Text.Json;
using Redot_Documentation.Versioning;

namespace Redot_Documentation.Services;

public class VersionManagerService
{
    public string[] Versions => _versions.Keys.ToArray();
    private Dictionary<string, VersionProvider> _versions = new();
    private List<string> _configuredVersions = new();
    private const string VersionFileName = "./docs/Versions.json";


    public VersionManagerService()
    {
        if (File.Exists(VersionFileName))
        {
            string fileContent = File.ReadAllText(VersionFileName);
            _configuredVersions = JsonSerializer.Deserialize<List<string>>(fileContent) ?? new List<string>() { "latest" };
        }
        else
        {
            _configuredVersions.Add("latest");
        }
    }

    public void LoadContent()
    {
        foreach (string versionName in _configuredVersions)
        {
            var version = new VersionProvider(versionName);
            version.ParseSlugs();
            _versions.Add(versionName, version);
        }
    }

    public VersionProvider GetVersionProvider(string version) => _versions[version];

}