using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.IntegrationTests.Fixtures;
using Testcontainers.MsSql;
using Xunit;

[assembly: AssemblyFixture(typeof(SqlServerFixture))]

namespace Microsoft.eShopWeb.IntegrationTests.Fixtures;

/// <summary>
/// Starts one SQL Server container for the whole test assembly. It is built and started on
/// first use, so tests that never ask for it run without a container runtime. Each test gets
/// its own empty database from <see cref="CreateCatalogContext"/>, so tests can't see each
/// other's rows.
/// </summary>
/// <remarks>
/// Requires a container runtime such as Docker Desktop or Podman, the same one the README
/// already lists for running the app. Without one, the tests that use this fixture fail
/// rather than skip, so CI can't pass by silently not running them.
/// </remarks>
public sealed class SqlServerFixture : IAsyncDisposable
{
    // Matches the image in docker-compose.yml.
    private const string Image = "mcr.microsoft.com/mssql/server:2022-latest";

    // Build() as well as StartAsync() needs a container runtime, so both wait for first use.
    // No cancellation token: the start is shared, so one test's cancellation must not fail
    // every other test that waits on it.
    private readonly Lazy<Task<MsSqlContainer>> _container = new(async () =>
    {
        var container = new MsSqlBuilder(Image).Build();
        await container.StartAsync();
        return container;
    });

    /// <summary>
    /// Returns a context for a new, uniquely named database that doesn't exist yet. Migrating
    /// the context, or calling EnsureCreated, creates it.
    /// </summary>
    public async Task<CatalogContext> CreateCatalogContext()
    {
        var container = await _container.Value;

        var connectionString = new SqlConnectionStringBuilder(container.GetConnectionString())
        {
            InitialCatalog = $"CatalogTests_{Guid.NewGuid():N}",
        }.ConnectionString;

        var options = new DbContextOptionsBuilder<CatalogContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new CatalogContext(options);
    }

    public async ValueTask DisposeAsync()
    {
        if (_container.IsValueCreated && _container.Value.IsCompletedSuccessfully)
        {
            await _container.Value.Result.DisposeAsync();
        }
    }
}
