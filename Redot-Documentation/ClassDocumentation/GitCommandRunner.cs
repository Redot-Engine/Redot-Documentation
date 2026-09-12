using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Redot_Documentation.ClassDocumentation;

/// <summary>Runs Git commands for class-documentation synchronization.</summary>
public interface IGitCommandRunner
{
    /// <summary>Runs a Git command.</summary>
    /// <param name="arguments">The Git arguments.</param>
    /// <param name="workingDirectory">The process working directory.</param>
    /// <param name="timeout">The maximum execution time.</param>
    /// <param name="cancellationToken">Cancels command execution.</param>
    /// <returns>The captured process output.</returns>
    /// <exception cref="OperationCanceledException">Execution is canceled.</exception>
    /// <exception cref="TimeoutException">Execution exceeds <paramref name="timeout"/>.</exception>
    /// <exception cref="InvalidOperationException">Git cannot start or exits unsuccessfully.</exception>
    Task<GitCommandResult> RunAsync(
        IReadOnlyList<string> arguments,
        string workingDirectory,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}

/// <summary>Contains captured Git process output.</summary>
/// <param name="StandardOutput">The trimmed standard output.</param>
/// <param name="StandardError">The trimmed standard error.</param>
public sealed record GitCommandResult(string StandardOutput, string StandardError);

/// <summary>Runs Git in a noninteractive child process.</summary>
public sealed class GitCommandRunner : IGitCommandRunner
{
    /// <summary>Matches URL authorities containing user information.</summary>
    private static readonly Regex CredentialUrlRegex = new(
        @"(?<scheme>[a-z][a-z0-9+.-]*://)[^/\s?#]*@",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public async Task<GitCommandResult> RunAsync(
        IReadOnlyList<string> arguments,
        string workingDirectory,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.Environment["GIT_TERMINAL_PROMPT"] = "0";
        foreach (string argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo };
        try
        {
            if (!process.Start())
                throw new InvalidOperationException("Git could not be started.");
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            throw new InvalidOperationException(
                "Git is required to synchronize class documentation but could not be started.",
                exception);
        }

        Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        Task<string> standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);

        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            TryKill(process);
            throw new TimeoutException($"Git did not complete within {timeout}.");
        }
        catch
        {
            TryKill(process);
            throw;
        }

        string standardOutput = await standardOutputTask;
        string standardError = await standardErrorTask;
        if (process.ExitCode != 0)
        {
            string command = string.Join(' ', arguments.Select(argument => QuoteForDisplay(RedactCredentials(argument))));
            string redactedStandardError = RedactCredentials(standardError.Trim());
            throw new InvalidOperationException(
                $"Git command 'git {command}' failed with exit code {process.ExitCode}: {redactedStandardError}");
        }

        return new GitCommandResult(standardOutput.Trim(), standardError.Trim());
    }

    /// <summary>Terminates a running process when possible.</summary>
    /// <param name="process">The process to terminate.</param>
    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
            // The process exited between the checks.
        }
    }

    /// <summary>Quotes a command argument for display.</summary>
    /// <param name="argument">The argument to quote.</param>
    /// <returns>The display-safe argument.</returns>
    private static string QuoteForDisplay(string argument)
        => argument.Any(char.IsWhiteSpace) ? $"\"{argument.Replace("\"", "\\\"", StringComparison.Ordinal)}\"" : argument;

    /// <summary>Redacts URL user information from text.</summary>
    /// <param name="value">The text to sanitize.</param>
    /// <returns>The sanitized text.</returns>
    private static string RedactCredentials(string value)
        => CredentialUrlRegex.Replace(value, "${scheme}***@");
}
