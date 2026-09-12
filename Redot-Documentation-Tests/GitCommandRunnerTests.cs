using Redot_Documentation.ClassDocumentation;

namespace Redot_Documentation_Tests;

public sealed class GitCommandRunnerTests
{
    [Fact]
    public async Task RunAsync_RedactsRepositoryCredentialsFromFailures()
    {
        var runner = new GitCommandRunner();

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runner.RunAsync(
                ["ls-remote", "https://username:super-secret@127.0.0.1:1/repository.git"],
                Path.GetTempPath(),
                TimeSpan.FromSeconds(10),
                CancellationToken.None));

        Assert.DoesNotContain("username", exception.Message);
        Assert.DoesNotContain("super-secret", exception.Message);
        Assert.Contains("https://***@127.0.0.1:1/repository.git", exception.Message);
        Assert.Contains("failed with exit code", exception.Message);
    }
}
