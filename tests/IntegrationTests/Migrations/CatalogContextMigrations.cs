using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.IntegrationTests.Fixtures;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Migrations;

public class CatalogContextMigrations(SqlServerFixture sqlServer)
{
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
        var actual = await SqlServerSchema.DescribeAsync(migrated, cancellationToken);
        // Guards against a catalog query that silently returns nothing for both databases. The
        // model has no check constraints yet, so that section is allowed to be empty.
        Assert.All(
            SqlServerSchema.Sections.Where(section => section != "check"),
            section => Assert.Contains(expected, line => line.StartsWith($"{section} ")));

        // List every drifted line in full; Assert.Equal stops at the first and truncates it.
        var drift = expected.Except(actual).Select(line => $"- {line}")
            .Concat(actual.Except(expected).Select(line => $"+ {line}"))
            .ToList();
        Assert.True(
            drift.Count == 0,
            $"Migrated schema (+) differs from the model's schema (-):\n{string.Join('\n', drift)}");
    }
}
