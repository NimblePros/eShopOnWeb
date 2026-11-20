using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.IntegrationTests.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Data.CatalogContextSeedTests;

public class SeedAsyncOnSqlServer(SqlServerFixture sqlServer)
{
    [Fact]
    public async Task MigratesAndSeedsAnEmptyDatabase()
    {
        // This is the app's startup path: on SQL Server, SeedAsync applies migrations first.
        // retry: MaxRetries skips its retry loop, so a failure surfaces at once and unmasked.
        const int MaxRetries = 10;
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var context = await sqlServer.CreateCatalogContext();

        await CatalogContextSeed.SeedAsync(context, NullLogger.Instance, retry: MaxRetries);

        Assert.Empty(await context.Database.GetPendingMigrationsAsync(cancellationToken));
        Assert.Equal(5, await context.CatalogBrands.CountAsync(cancellationToken));
        Assert.Equal(4, await context.CatalogTypes.CountAsync(cancellationToken));
        Assert.Equal(12, await context.CatalogItems.CountAsync(cancellationToken));
    }
}
