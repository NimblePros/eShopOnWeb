using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Data.CatalogContextSeedTests;

public class SeedAsync
{
    [Fact]
    public async Task CompletesAndSeedsCatalogWhenRetrySucceedsAfterTransientFailure()
    {
        var interceptor = new FailingSaveChangesInterceptor(failuresBeforeSuccess: 1);
        using var context = CreateContext(interceptor);

        await CatalogContextSeed.SeedAsync(context, NullLogger.Instance);

        Assert.Equal(1, interceptor.Failures);
        Assert.Equal(5, await context.CatalogBrands.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(4, await context.CatalogTypes.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(12, await context.CatalogItems.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ThrowsWhenEveryRetryFails()
    {
        var interceptor = new FailingSaveChangesInterceptor(failuresBeforeSuccess: int.MaxValue);
        using var context = CreateContext(interceptor);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CatalogContextSeed.SeedAsync(context, NullLogger.Instance));

        // Initial attempt plus 10 retries.
        Assert.Equal(11, interceptor.Failures);
    }

    private static CatalogContext CreateContext(IInterceptor interceptor)
    {
        var options = new DbContextOptionsBuilder<CatalogContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;
        return new CatalogContext(options);
    }

    private sealed class FailingSaveChangesInterceptor(int failuresBeforeSuccess) : SaveChangesInterceptor
    {
        public int Attempts { get; private set; }
        public int Failures { get; private set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Attempts++;
            if (Attempts <= failuresBeforeSuccess)
            {
                Failures++;
                throw new InvalidOperationException("Simulated transient database failure.");
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
