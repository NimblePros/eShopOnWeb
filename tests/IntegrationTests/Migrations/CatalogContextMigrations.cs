using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.IntegrationTests.Fixtures;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Migrations;

public class CatalogContextMigrations(SqlServerFixture sqlServer)
{
    // Defaults that AlterColumn(..., defaultValue: ...) left behind when older migrations made
    // these columns non-nullable. The model declares none of them. #381 tracks dropping them;
    // once it does, the test below fails until the entries are removed from here too.
    private static readonly Dictionary<string, string> KnownLeftoverDefaults = new()
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
    public async Task AllMigrationsApplyToAnEmptyDatabase()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var context = await sqlServer.CreateCatalogContext();

        await context.Database.MigrateAsync(cancellationToken);

        Assert.Equal(
            context.Database.GetMigrations(),
            await context.Database.GetAppliedMigrationsAsync(cancellationToken));
        Assert.Empty(await context.Database.GetPendingMigrationsAsync(cancellationToken));
    }

    [Fact]
    public async Task MigratedSchemaMatchesTheSchemaBuiltFromTheModel()
    {
        // HasPendingModelChanges compares the model with the snapshot only. A migration whose
        // Up() disagrees with its own designer or the snapshot gets past it, but not past this.
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var migrated = await sqlServer.CreateCatalogContext();
        await using var fromModel = await sqlServer.CreateCatalogContext();

        await migrated.Database.MigrateAsync(cancellationToken);
        await fromModel.Database.EnsureCreatedAsync(cancellationToken);

        var expected = await SqlServerSchema.DescribeAsync(fromModel, cancellationToken);
        var actual = (await SqlServerSchema.DescribeAsync(migrated, cancellationToken)).ToList();
        // Guards against a catalog query that silently returns nothing for both databases. The
        // model has no check constraints yet, so that section is allowed to be empty.
        Assert.All(
            SqlServerSchema.Sections.Where(section => section != "check"),
            section => Assert.Contains(expected, line => line.StartsWith($"{section} ")));

        foreach (var (column, definition) in KnownLeftoverDefaults)
        {
            var index = actual.FindIndex(line =>
                line.StartsWith($"column {column}: ") && line.EndsWith($" DEFAULT {definition}"));
            Assert.True(
                index >= 0,
                $"{column} no longer has the default {definition}. Remove it from {nameof(KnownLeftoverDefaults)}.");
            actual[index] = actual[index][..^definition.Length] + "-";
        }

        // List every drifted line in full; Assert.Equal stops at the first and truncates it.
        var drift = expected.Except(actual).Select(line => $"- {line}")
            .Concat(actual.Except(expected).Select(line => $"+ {line}"))
            .ToList();
        Assert.True(
            drift.Count == 0,
            $"Migrated schema (+) differs from the model's schema (-):\n{string.Join('\n', drift)}");
    }
}
