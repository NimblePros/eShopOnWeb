using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.IntegrationTests.Fixtures;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Migrations;

public class DropLeftoverDefaultConstraintsMigration(SqlServerFixture sqlServer)
{
    private const string MigrationId = "20261119140000_DropLeftoverDefaultConstraints";
    private const string PreviousMigrationId = "20261119130000_ClampBasketItemQuantityToValidRange";

    // The defaults earlier migrations left behind, as SQL Server reports them.
    private static readonly Dictionary<string, string> LeftoverDefaults = new()
    {
        ["Catalog.Description"] = "(N'')",
        ["OrderItems.ItemOrdered_CatalogItemId"] = "((0))",
        ["OrderItems.ItemOrdered_PictureUri"] = "(N'')",
        ["OrderItems.ItemOrdered_ProductName"] = "(N'')",
        ["Orders.BuyerId"] = "(N'')",
        ["Orders.ShipToAddress_City"] = "(N'')",
        ["Orders.ShipToAddress_Country"] = "(N'')",
        ["Orders.ShipToAddress_State"] = "(N'')",
        ["Orders.ShipToAddress_Street"] = "(N'')",
        ["Orders.ShipToAddress_ZipCode"] = "(N'')",
    };

    [Fact]
    public async Task UpDropsTheLeftoverDefaultsAndDownRestoresThem()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var context = await sqlServer.CreateCatalogContext();
        var migrator = context.GetService<IMigrator>();

        await migrator.MigrateAsync(PreviousMigrationId, cancellationToken);
        Assert.Equal(LeftoverDefaults, await DescribeDefaultsAsync(context, cancellationToken));

        await migrator.MigrateAsync(MigrationId, cancellationToken);
        Assert.Empty(await DescribeDefaultsAsync(context, cancellationToken));

        await migrator.MigrateAsync(PreviousMigrationId, cancellationToken);
        Assert.Equal(LeftoverDefaults, await DescribeDefaultsAsync(context, cancellationToken));
    }

    [Fact]
    public async Task AnOrderInsertedWithoutABuyerIsRejected()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var context = await sqlServer.CreateCatalogContext();
        await context.Database.MigrateAsync(cancellationToken);

        // Before #381 the leftover default stored this row with an empty BuyerId.
        var exception = await Assert.ThrowsAsync<SqlException>(() => context.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO [Orders] ([OrderDate], [ShipToAddress_City], [ShipToAddress_Country],
                [ShipToAddress_State], [ShipToAddress_Street], [ShipToAddress_ZipCode])
            VALUES (SYSDATETIMEOFFSET(), 'City', 'Country', 'State', 'Street', '12345');
            """,
            cancellationToken));

        Assert.Contains("BuyerId", exception.Message);
    }

    private static async Task<Dictionary<string, string>> DescribeDefaultsAsync(
        CatalogContext context,
        CancellationToken cancellationToken)
    {
        const string marker = " DEFAULT ";
        return (await SqlServerSchema.DescribeAsync(context, cancellationToken))
            .Where(line => line.StartsWith("column ") && !line.EndsWith($"{marker}-"))
            .ToDictionary(
                line => line["column ".Length..line.IndexOf(": ")],
                line => line[(line.LastIndexOf(marker) + marker.Length)..]);
    }
}
