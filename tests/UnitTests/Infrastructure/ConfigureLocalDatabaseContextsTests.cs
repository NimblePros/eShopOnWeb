using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Infrastructure;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NimblePros.Metronome;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.Infrastructure;

public class ConfigureLocalDatabaseContextsTests
{
    // Written the way SqlClient normalizes connection strings; EF Core then appends an Application Name.
    private const string CatalogConnectionString = "Data Source=sqlserver,1433;Initial Catalog=Catalog;Trust Server Certificate=True";
    private const string IdentityConnectionString = "Data Source=sqlserver,1433;Initial Catalog=Identity;Trust Server Certificate=True";

    [Fact]
    public void UsesSqlServerWithConfiguredConnectionStrings()
    {
        using var provider = BuildServiceProvider(new()
        {
            [$"ConnectionStrings:{Dependencies.CATALOG_CONNECTION_NAME}"] = CatalogConnectionString,
            [$"ConnectionStrings:{Dependencies.IDENTITY_CONNECTION_NAME}"] = IdentityConnectionString,
        });
        using var scope = provider.CreateScope();

        var catalogContext = scope.ServiceProvider.GetRequiredService<CatalogContext>();
        var identityContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();

        Assert.True(catalogContext.Database.IsSqlServer());
        Assert.StartsWith(CatalogConnectionString, catalogContext.Database.GetConnectionString());
        Assert.True(identityContext.Database.IsSqlServer());
        Assert.StartsWith(IdentityConnectionString, identityContext.Database.GetConnectionString());
    }

    [Fact]
    public void ThrowsDescriptiveErrorWhenCatalogConnectionStringIsMissing()
    {
        using var provider = BuildServiceProvider(new()
        {
            [$"ConnectionStrings:{Dependencies.IDENTITY_CONNECTION_NAME}"] = IdentityConnectionString,
        });
        using var scope = provider.CreateScope();

        var exception = Assert.Throws<DatabaseNotConfiguredException>(
            () => scope.ServiceProvider.GetRequiredService<CatalogContext>());

        Assert.Equal("CatalogConnection", exception.ConnectionName);
        Assert.Contains("'CatalogConnection'", exception.Message);
        Assert.Contains("eShopWeb.AppHost", exception.Message);
    }

    [Fact]
    public void ThrowsDescriptiveErrorWhenIdentityConnectionStringIsMissing()
    {
        using var provider = BuildServiceProvider(new()
        {
            [$"ConnectionStrings:{Dependencies.CATALOG_CONNECTION_NAME}"] = CatalogConnectionString,
        });
        using var scope = provider.CreateScope();

        var exception = Assert.Throws<DatabaseNotConfiguredException>(
            () => scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>());

        Assert.Equal("IdentityConnection", exception.ConnectionName);
        Assert.Contains("'IdentityConnection'", exception.Message);
        Assert.Contains("eShopWeb.AppHost", exception.Message);
    }

    [Fact]
    public void DoesNotRequireConnectionStringsForEfDesignTimeTools()
    {
        // "dotnet ef migrations add" builds the contexts without connecting to a database.
        using var provider = BuildServiceProvider([]);
        using var scope = provider.CreateScope();

        var wasDesignTime = EF.IsDesignTime;
        EF.IsDesignTime = true;
        try
        {
            Assert.True(scope.ServiceProvider.GetRequiredService<CatalogContext>().Database.IsSqlServer());
            Assert.True(scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>().Database.IsSqlServer());
        }
        finally
        {
            EF.IsDesignTime = wasDesignTime;
        }
    }

    [Fact]
    public void UsesInMemoryDatabaseWithoutConnectionStringsWhenConfigured()
    {
        using var provider = BuildServiceProvider(new()
        {
            ["UseOnlyInMemoryDatabase"] = "true",
        });
        using var scope = provider.CreateScope();

        Assert.True(scope.ServiceProvider.GetRequiredService<CatalogContext>().Database.IsInMemory());
        Assert.True(scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>().Database.IsInMemory());
    }

    private static ServiceProvider BuildServiceProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMetronome();
        services.ConfigureLocalDatabaseContexts(configuration);
        return services.BuildServiceProvider();
    }
}
