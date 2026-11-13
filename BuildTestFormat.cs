#!/usr/bin/env dotnet
// Run this with:
// dotnet ./BuildTestFormat.cs

using System.Diagnostics;
using System.Text.RegularExpressions;

string solutionRoot = FindSolutionRoot();
string solutionFile = Path.Combine(solutionRoot, "eShopOnWeb.slnx");
string testsRoot = Path.Combine(solutionRoot, "tests");

var overall = Stopwatch.StartNew();
try
{
    Console.WriteLine("Restoring...");
    CommandResult restoreResult = await RunAsync("dotnet", $"restore \"{solutionFile}\"", solutionRoot);
    if (restoreResult.ExitCode != 0)
    {
        WriteFailureDetails("Restore", restoreResult);
        return restoreResult.ExitCode;
    }

    Console.WriteLine($"Restoring... completed in {FormatElapsed(restoreResult.Elapsed)}");

    Console.WriteLine("Building...");
    CommandResult buildResult = await RunAsync("dotnet", $"build \"{solutionFile}\" --no-restore", solutionRoot);
    if (buildResult.ExitCode != 0)
    {
        WriteFailureDetails("Build", buildResult);
        return buildResult.ExitCode;
    }

    Console.WriteLine($"Building... completed in {FormatElapsed(buildResult.Elapsed)}");

    Console.WriteLine("Executing tests...");
    foreach (string projectPath in GetTestProjects(testsRoot))
    {
        string projectName = GetProjectName(projectPath);
        string projectDirectory = Path.GetDirectoryName(projectPath)!;

        // global.json selects Microsoft.Testing.Platform, which requires --project.
        CommandResult testResult = await RunAsync("dotnet", $"test --no-build --project \"{projectPath}\"", projectDirectory);

        bool succeeded = testResult.ExitCode == 0;
        TestSummary? summary = ParseTestSummary(testResult.StdOut + Environment.NewLine + testResult.StdErr);
        string outcome = summary is null
            ? (succeeded ? "passed" : "failed")
            : summary.Describe();
        Console.WriteLine($"  {projectName}... completed in {FormatElapsed(testResult.Elapsed)}: {(succeeded ? "✅" : "❌")} {outcome}");

        if (!succeeded)
        {
            WriteFailureDetails("Test", testResult);
            return testResult.ExitCode;
        }
    }

    Console.WriteLine("Checking formatting...");
    CommandResult formatResult = await RunAsync("dotnet", $"format \"{solutionFile}\" --verify-no-changes", solutionRoot);
    if (formatResult.ExitCode != 0)
    {
        Console.WriteLine($"Checking formatting... completed in {FormatElapsed(formatResult.Elapsed)}: ❌ changes needed (run `dotnet format`)");
        WriteFailureDetails("Format", formatResult);
        return formatResult.ExitCode;
    }

    Console.WriteLine($"Checking formatting... completed in {FormatElapsed(formatResult.Elapsed)}: ✅ clean");

    return 0;
}
finally
{
    overall.Stop();
    Console.WriteLine($"Completed in {FormatElapsed(overall.Elapsed)}");
}

static async Task<CommandResult> RunAsync(string file, string args, string workingDirectory)
{
    var start = Stopwatch.StartNew();
    using var process = Process.Start(
      new ProcessStartInfo(file, args)
      {
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          WorkingDirectory = workingDirectory
      })!;

    Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
    Task<string> stderrTask = process.StandardError.ReadToEndAsync();

    await process.WaitForExitAsync();

    string stdout = await stdoutTask;
    string stderr = await stderrTask;

    start.Stop();
    return new CommandResult(process.ExitCode, start.Elapsed, stdout, stderr);
}

static string FormatElapsed(TimeSpan elapsed)
    => elapsed.TotalMinutes >= 1
        ? $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s"
        : $"{elapsed.Seconds}s";

// Parses the Microsoft.Testing.Platform run summary:
//   total: 47
//   failed: 0
//   succeeded: 47
//   skipped: 0
static TestSummary? ParseTestSummary(string output)
{
    int? total = GetCount("total");
    int? failed = GetCount("failed");
    int? passed = GetCount("succeeded");
    int? skipped = GetCount("skipped");

    return total is null || failed is null || passed is null
        ? null
        : new TestSummary(passed.Value, failed.Value, skipped ?? 0, total.Value);

    int? GetCount(string label)
    {
        Match match = Regex.Match(output, $@"^\s*{label}:\s*(\d+)\s*$", RegexOptions.Multiline);
        return match.Success ? int.Parse(match.Groups[1].Value) : null;
    }
}

static void WriteFailureDetails(string step, CommandResult result)
{
    Console.WriteLine();
    Console.WriteLine($"{step} failure details:");

    string combinedOutput = string.Join(Environment.NewLine, new[] { result.StdOut, result.StdErr }
        .Where(text => !string.IsNullOrWhiteSpace(text)));

    Console.WriteLine(string.IsNullOrWhiteSpace(combinedOutput)
        ? "  (no output captured)"
        : string.Join(Environment.NewLine, combinedOutput.Split(Environment.NewLine, StringSplitOptions.None)
            .Select(line => $"  {line}")));
}

static string GetProjectName(string projectPath)
    => Path.GetFileNameWithoutExtension(projectPath);

static IEnumerable<string> GetTestProjects(string testsRoot)
    => Directory.EnumerateFiles(testsRoot, "*Tests.csproj", SearchOption.AllDirectories)
        .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

static string FindSolutionRoot()
{
    string? current = Environment.CurrentDirectory;
    while (!string.IsNullOrWhiteSpace(current))
    {
        if (File.Exists(Path.Combine(current, "eShopOnWeb.slnx")))
        {
            return current;
        }

        current = Directory.GetParent(current)?.FullName;
    }

    throw new DirectoryNotFoundException("Could not locate repository root containing eShopOnWeb.slnx.");
}

record CommandResult(int ExitCode, TimeSpan Elapsed, string StdOut, string StdErr);

record TestSummary(int Passed, int Failed, int Skipped, int Total)
{
    public string Describe()
    {
        string text = $"{Passed}/{Total} passing";
        if (Failed > 0)
        {
            text += $", {Failed} failed";
        }
        if (Skipped > 0)
        {
            text += $", {Skipped} skipped";
        }
        return text;
    }
}
