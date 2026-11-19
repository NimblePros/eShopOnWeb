using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Data.Migrations;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Migrations;

public class ClampBasketItemQuantityToValidRangeMigration
{
    private const string MigrationId = "20261119130000_ClampBasketItemQuantityToValidRange";

    [Fact]
    public void UpClampsBasketItemQuantitiesIntoZeroThroughTheCurrentMax()
    {
        var migration = new ClampBasketItemQuantityToValidRange();

        var sql = migration.UpOperations.Select(op => Assert.IsType<SqlOperation>(op).Sql);
        Assert.Equal(
            [
                $"UPDATE [BasketItems] SET [Quantity] = {BasketItem.MaxQuantity} WHERE [Quantity] > {BasketItem.MaxQuantity};",
                "UPDATE [BasketItems] SET [Quantity] = 0 WHERE [Quantity] < 0;",
            ],
            sql);
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
