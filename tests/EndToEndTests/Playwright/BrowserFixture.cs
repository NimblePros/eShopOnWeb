using System.Diagnostics;
using System.Net.Http;
using Microsoft.Playwright;
using Xunit;

namespace Microsoft.eShopWeb.EndToEndTests.Playwright;

public sealed class BrowserFixture : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private string _solutionRoot = string.Empty;
    private Process? _webProcess;
    private IPlaywright? _playwright;

    public IBrowser Browser { get; private set; } = default!;
    public string BaseUrl { get; private set; } = string.Empty;
    public string ArtifactsRootPath { get; private set; } = string.Empty;

    public BrowserFixture()
    {
        _httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        })
        {
            Timeout = TimeSpan.FromSeconds(1)
        };
    }

    public async ValueTask InitializeAsync()
    {
        _solutionRoot = FindSolutionRoot();
        ArtifactsRootPath = Path.Combine(_solutionRoot, "TestResults", "PlaywrightArtifacts");
        Directory.CreateDirectory(ArtifactsRootPath);

        BaseUrl = await StartWebAppAsync(_solutionRoot);

        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task<(IBrowserContext Context, string OutputDirectory)> CreateDebugContextAsync(string testName)
    {
        string testDirectory = Path.Combine(
            ArtifactsRootPath,
            $"{SanitizePathSegment(testName)}-{DateTime.UtcNow:yyyyMMdd-HHmmss}");
        Directory.CreateDirectory(testDirectory);

        string videoDirectory = Path.Combine(testDirectory, "video");
        Directory.CreateDirectory(videoDirectory);

        IBrowserContext context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            RecordVideoDir = videoDirectory
        });

        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        return (context, testDirectory);
    }

    public async Task CaptureFailureArtifactsAsync(IBrowserContext context, IPage page, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(outputDirectory, "failure.png"),
            FullPage = true
        });

        await context.Tracing.StopAsync(new TracingStopOptions
        {
            Path = Path.Combine(outputDirectory, "trace.zip")
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.DisposeAsync();
        }

        _playwright?.Dispose();

        _httpClient.Dispose();

        if (_webProcess is { HasExited: false })
        {
            _webProcess.Kill(entireProcessTree: true);
            await _webProcess.WaitForExitAsync();
        }
    }

    private async Task<string> StartWebAppAsync(string solutionRoot)
    {
        const string baseUrl = "https://localhost:5001";
        string webProject = Path.Combine(solutionRoot, "src", "Web", "Web.csproj");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --no-build --project \"{webProject}\"",
            WorkingDirectory = solutionRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        // Use the in-memory database so the site doesn't need SQL Server / LocalDB
        // (LocalDB doesn't exist on Linux CI runners).
        startInfo.Environment["UseOnlyInMemoryDatabase"] = "true";

        _webProcess = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start Web app process.");

        await WaitForServerAsync(baseUrl, TimeSpan.FromSeconds(90));
        return baseUrl;
    }

    private async Task WaitForServerAsync(string baseUrl, TimeSpan timeout)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(timeout);

        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                if (_webProcess is { HasExited: true })
                {
                    string errorOutput = await _webProcess.StandardError.ReadToEndAsync();
                    throw new InvalidOperationException($"Web app exited during startup. {errorOutput}".Trim());
                }

                using HttpResponseMessage response = await _httpClient.GetAsync(baseUrl);
                if ((int)response.StatusCode > 0)
                {
                    return;
                }
            }
            catch
            {
                // App is still starting.
            }

            await Task.Delay(250);
        }

        if (_webProcess is { HasExited: false })
        {
            _webProcess.Kill(entireProcessTree: true);
            await _webProcess.WaitForExitAsync();
        }

        throw new TimeoutException($"Web app did not start within {timeout.TotalSeconds} seconds.");
    }

    private static string SanitizePathSegment(string value)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        Span<char> buffer = stackalloc char[value.Length];

        int i = 0;
        foreach (char c in value)
        {
            buffer[i++] = invalidChars.Contains(c) ? '-' : c;
        }

        return buffer[..i].ToString();
    }

    private static string FindSolutionRoot()
    {
        string? current = AppContext.BaseDirectory;
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
}
