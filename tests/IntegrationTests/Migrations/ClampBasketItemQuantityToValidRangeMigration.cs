using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Data.Migrations;
using Microsoft.eShopWeb.IntegrationTests.Fixtures;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Migrations;

public class ClampBasketItemQuantityToValidRangeMigration(SqlServerFixture sqlServer)
{
    private const string MigrationId = "20261119130000_ClampBasketItemQuantityToValidRange";
    private const string PreviousMigrationId = "20250310153034_Updates";

    [Fact]
    public async Task UpClampsStoredBasketItemQuantitiesIntoZeroThroughTheMax()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var context = await sqlServer.CreateCatalogContext();
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync(PreviousMigrationId, cancellationToken);

        // Raw SQL, because these rows break the rules the entities enforce today.
        await context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO [Baskets] ([BuyerId]) VALUES ('legacy-buyer');
            INSERT INTO [BasketItems] ([BasketId], [CatalogItemId], [Quantity], [UnitPrice])
            SELECT [Id], [CatalogItemId], [Quantity], 1.50
            FROM [Baskets]
            CROSS JOIN (VALUES (1, 2147483647), (2, 5000), (3, 1000), (4, 999), (5, 1), (6, 0), (7, -3), (8, -2147483648))
                AS [Lines] ([CatalogItemId], [Quantity]);
            """,
            cancellationToken);

        await migrator.MigrateAsync(MigrationId, cancellationToken);

        var quantities = await context.Database
            .SqlQueryRaw<int>("SELECT [Quantity] AS [Value] FROM [BasketItems] ORDER BY [CatalogItemId]")
            .ToListAsync(cancellationToken);
        Assert.Equal([999, 999, 999, 999, 1, 0, 0, 0], quantities);
    }

    [Fact]
    public void DownDoesNothingBecauseOriginalQuantitiesAreNotRecoverable()
    {
        var migration = new ClampBasketItemQuantityToValidRange();

        Assert.Empty(migration.DownOperations);
    }

    [Fact]
    public void CatalogContextDiscoversTheMigrationAndHasNoPendingModelChanges()
    {
        // The SQL Server provider resolves migrations and the model without opening a connection.
        var options = new DbContextOptionsBuilder<CatalogContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options;
        using var context = new CatalogContext(options);

        Assert.Contains(MigrationId, context.Database.GetMigrations());
        Assert.False(context.Database.HasPendingModelChanges());
    }
}
