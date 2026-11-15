#!/usr/bin/env dotnet
// Renders a merged Cobertura report as Markdown for the PR coverage comment: one summary row per src
// project, plus a collapsible folder/file tree per project that mirrors src/.
//
// ReportGenerator does the merging (one report per test project in, one Cobertura report out). This
// script only builds the tree, because ReportGenerator's Markdown summaries group by assembly and class,
// not by folder.
//
// Run this with:
// dotnet run --file .github/scripts/CoverageReport.cs -- <merged-cobertura-file> <output-markdown-file>

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

const double LowerThreshold = 50;
const double UpperThreshold = 75;
// GitHub rejects comments over 65,536 characters; fall back to folders only past this size.
const int MaxCommentLength = 60_000;

string reportFile = args.Length > 0 ? args[0] : "coverage/report/Cobertura.xml";
string outputFile = args.Length > 1 ? args[1] : "code-coverage-results.md";
string repoRoot = Directory.GetCurrentDirectory();

if (!File.Exists(reportFile))
{
    Console.Error.WriteLine($"Cobertura report '{reportFile}' not found.");
    return 1;
}

// Key: src-relative file path (e.g. "Web/Pages/Index.cshtml.cs"). Value: line number -> line coverage.
var files = new SortedDictionary<string, Dictionary<int, LineCoverage>>(StringComparer.Ordinal);
var conditionCoverage = new Regex(@"\((\d+)/(\d+)\)");

XDocument report = XDocument.Load(reportFile);
string[] sources = report.Descendants("source").Select(s => s.Value).ToArray();

foreach (XElement cls in report.Descendants("class"))
{
    string? srcPath = ResolveSrcPath((string?)cls.Attribute("filename"), sources);
    if (srcPath is null)
    {
        continue;
    }

    // A file can hold several classes (nested or partial), so its lines can come from several <class> elements.
    if (!files.TryGetValue(srcPath, out var lines))
    {
        lines = [];
        files[srcPath] = lines;
    }

    // Class-level <lines> repeats every method's lines, so read only those.
    foreach (XElement line in cls.Elements("lines").Elements("line"))
    {
        int number = (int)line.Attribute("number")!;
        bool hit = long.Parse((string)line.Attribute("hits")!, CultureInfo.InvariantCulture) > 0;
        int branchesCovered = 0, branchesTotal = 0;
        Match match = conditionCoverage.Match((string?)line.Attribute("condition-coverage") ?? "");
        if (match.Success)
        {
            branchesCovered = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            branchesTotal = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        }

        lines.TryAdd(number, new LineCoverage(hit, branchesCovered, branchesTotal));
    }
}

if (files.Count == 0)
{
    Console.Error.WriteLine($"'{reportFile}' contained no files under src/.");
    return 1;
}

// Build the tree: src project -> folders -> files.
var root = new Node("src");
foreach ((string path, var lines) in files)
{
    Node node = root;
    foreach (string segment in path.Split('/'))
    {
        node = node.Child(segment);
    }

    node.Stats = new Stats(
        lines.Values.Count(l => l.Hit),
        lines.Count,
        lines.Values.Sum(l => l.BranchesCovered),
        lines.Values.Sum(l => l.BranchesTotal));
}
root.Aggregate();

string markdown = Render(includeFiles: true);
if (markdown.Length > MaxCommentLength)
{
    markdown = Render(includeFiles: false);
}

File.WriteAllText(outputFile, markdown);
Console.WriteLine(markdown);
Console.WriteLine(
    $"Rendered {files.Count} src files: {Percent(root.Stats.LinesCovered, root.Stats.LinesTotal)} line coverage.");
return 0;

string Render(bool includeFiles)
{
    var sb = new StringBuilder();
    Stats total = root.Stats;
    double totalRate = Rate(total.LinesCovered, total.LinesTotal) ?? 0;
    string badgeColor = totalRate < LowerThreshold ? "critical" : totalRate < UpperThreshold ? "yellow" : "success";
    sb.AppendLine($"![Code Coverage](https://img.shields.io/badge/Code%20Coverage-{totalRate:0}%25-{badgeColor}?style=flat)");
    sb.AppendLine();
    sb.AppendLine("Merged across all test projects. A line counts as covered if any test project ran it. The full HTML report is in the `code-coverage-report` artifact on the workflow run.");
    sb.AppendLine();
    sb.AppendLine("Project | Line Rate | Branch Rate | Health");
    sb.AppendLine("------- | --------- | ----------- | ------");
    foreach (Node project in root.Children.Values)
    {
        sb.AppendLine($"{project.Name} | {LineCell(project.Stats)} | {BranchCell(project.Stats)} | {Health(project.Stats)}");
    }
    sb.AppendLine($"**Total** | **{LineCell(total)}** | **{BranchCell(total)}** | {Health(total)}");
    sb.AppendLine();

    foreach (Node project in root.Children.Values)
    {
        sb.AppendLine($"<details><summary><b>{WebUtility.HtmlEncode(project.Name)}</b>: {Percent(project.Stats.LinesCovered, project.Stats.LinesTotal)} of lines</summary>");
        sb.AppendLine();
        sb.AppendLine("Path | Line Rate | Branch Rate | Health");
        sb.AppendLine("---- | --------- | ----------- | ------");
        foreach (Node child in project.Children.Values)
        {
            WriteRows(sb, child, depth: 0, includeFiles);
        }
        sb.AppendLine();
        sb.AppendLine("</details>");
        sb.AppendLine();
    }

    if (!includeFiles)
    {
        sb.AppendLine("_File rows were left out to keep this comment under GitHub's size limit._");
    }

    return sb.ToString();
}

void WriteRows(StringBuilder sb, Node node, int depth, bool includeFiles)
{
    if (node.IsFile && !includeFiles)
    {
        return;
    }

    // Collapse single-folder chains like "Entities/BasketAggregate/" into one row.
    string name = node.Name;
    while (!node.IsFile && node.Children.Count == 1 && !node.Children.Values.First().IsFile)
    {
        node = node.Children.Values.First();
        name += "/" + node.Name;
    }

    string indent = string.Concat(Enumerable.Repeat("&nbsp;&nbsp;&nbsp;&nbsp;", depth));
    string label = node.IsFile ? $"📄 {Html(name)}" : $"📁 **{Html(name)}/**";
    sb.AppendLine($"{indent}{label} | {LineCell(node.Stats)} | {BranchCell(node.Stats)} | {Health(node.Stats)}");

    // Folders first, then files, each alphabetical.
    foreach (Node child in node.Children.Values.OrderBy(c => c.IsFile).ThenBy(c => c.Name, StringComparer.Ordinal))
    {
        WriteRows(sb, child, depth + 1, includeFiles);
    }
}

// Turns a Cobertura filename into a path relative to src/, or null when the file isn't part of this repo's src/
// (third-party packages, generated code under obj/).
string? ResolveSrcPath(string? filename, string[] sources)
{
    if (string.IsNullOrEmpty(filename))
    {
        return null;
    }

    IEnumerable<string> candidates = Path.IsPathRooted(filename)
        ? [filename]
        : sources.Select(source => Path.Combine(source, filename));
    foreach (string candidate in candidates)
    {
        string fullPath = Path.GetFullPath(candidate);
        if (!File.Exists(fullPath))
        {
            continue;
        }

        string relative = Path.GetRelativePath(repoRoot, fullPath).Replace('\\', '/');
        if (!relative.StartsWith("src/", StringComparison.Ordinal) || relative.Contains("/obj/", StringComparison.Ordinal))
        {
            return null;
        }

        return relative["src/".Length..];
    }

    return null;
}

static double? Rate(int covered, int total) => total == 0 ? null : 100.0 * covered / total;

static string Percent(int covered, int total) => Rate(covered, total) is double rate ? $"{Math.Floor(rate):0}%" : "n/a";

static string LineCell(Stats s) => $"{Percent(s.LinesCovered, s.LinesTotal)} ({s.LinesCovered}/{s.LinesTotal})";

static string BranchCell(Stats s) => s.BranchesTotal == 0 ? "n/a" : $"{Percent(s.BranchesCovered, s.BranchesTotal)} ({s.BranchesCovered}/{s.BranchesTotal})";

static string Health(Stats s)
{
    double rate = Rate(s.LinesCovered, s.LinesTotal) ?? 100;
    return rate < LowerThreshold ? "❌" : rate < UpperThreshold ? "➖" : "✔";
}

// Keep file names like "_Layout.cshtml" or "Generic<T>.cs" from turning into Markdown or HTML.
static string Html(string text) => WebUtility.HtmlEncode(text).Replace("_", "\\_").Replace("|", "\\|");

readonly record struct LineCoverage(bool Hit, int BranchesCovered, int BranchesTotal);

record struct Stats(int LinesCovered, int LinesTotal, int BranchesCovered, int BranchesTotal)
{
    public static Stats operator +(Stats a, Stats b) => new(
        a.LinesCovered + b.LinesCovered,
        a.LinesTotal + b.LinesTotal,
        a.BranchesCovered + b.BranchesCovered,
        a.BranchesTotal + b.BranchesTotal);
}

class Node(string name)
{
    public string Name { get; } = name;
    public SortedDictionary<string, Node> Children { get; } = new(StringComparer.Ordinal);
    public Stats Stats { get; set; }
    public bool IsFile => Children.Count == 0;

    public Node Child(string segment)
    {
        if (!Children.TryGetValue(segment, out Node? child))
        {
            child = new Node(segment);
            Children[segment] = child;
        }

        return child;
    }

    public Stats Aggregate()
    {
        if (!IsFile)
        {
            Stats = Children.Values.Aggregate(default(Stats), (sum, child) => sum + child.Aggregate());
        }

        return Stats;
    }
}
