using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.FunctionalTests.Web;

public class TestApplication : WebApplicationFactory<IBasketViewModelService>
{
    /// <summary>
    /// The auth cookie is issued with the Secure flag, so test clients must talk to the app over
    /// HTTPS or their cookie container won't send it back.
    /// </summary>
    public static readonly Uri HttpsBaseAddress = new("https://localhost");

    private readonly string _environment = "Development";

    /// <summary>Every log entry the app has written, for tests that check an operation is logged.</summary>
    public CapturingLoggerProvider Logs { get; } = new();

    public TestApplication()
    {
        ClientOptions.BaseAddress = HttpsBaseAddress;
    }

    public HttpClient CreateClient(bool allowAutoRedirect) =>
        CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = allowAutoRedirect,
            BaseAddress = HttpsBaseAddress
        });

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        client.BaseAddress = HttpsBaseAddress;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // No Seq server runs during tests, so the Seq OTLP exporters' final flush at host shutdown
        // keeps retrying until it times out (10s by default). A short timeout lets the host stop
        // promptly; these settings are read while Program.cs builds the app.
        builder.UseSetting("Aspire:Seq:Logs:TimeoutMilliseconds", "100");
        builder.UseSetting("Aspire:Seq:Traces:TimeoutMilliseconds", "100");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        builder.ConfigureLogging(logging => logging
            .AddProvider(Logs)
            .AddFilter<CapturingLoggerProvider>("Microsoft.eShopWeb", LogLevel.Information)
            .AddFilter<CapturingLoggerProvider>("Microsoft", LogLevel.Warning)
            .AddFilter<CapturingLoggerProvider>("System", LogLevel.Warning));

        // Add mock/test services to the builder here
        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(d =>
                                                d.ServiceType == typeof(DbContextOptions<CatalogContext>) ||
                                                d.ServiceType == typeof(DbContextOptions<AppIdentityDbContext>))
                                            .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddScoped(sp =>
            {
                // Replace SQLite with in-memory database for tests
                return new DbContextOptionsBuilder<CatalogContext>()
                .UseInMemoryDatabase("InMemoryDbForTesting")
                .UseApplicationServiceProvider(sp)
                .Options;
            });
            services.AddScoped(sp =>
            {
                // Replace SQLite with in-memory database for tests
                return new DbContextOptionsBuilder<AppIdentityDbContext>()
                .UseInMemoryDatabase("Identity")
                .UseApplicationServiceProvider(sp)
                .Options;
            });
        });

        return base.CreateHost(builder);
    }
}
