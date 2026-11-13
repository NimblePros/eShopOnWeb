using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.eShopWeb.AppHostTests;

// These tests inspect the AppHost's application model only. They don't start the app or any
// containers, so they run without Docker.
public class SqlServerResourceTests
{
    [Fact]
    public async Task AddsSqlServerContainerWithCatalogAndIdentityDatabases()
    {
        await using var builder = await CreateAppHostBuilderAsync();

        var sqlServer = Assert.Single(builder.Resources.OfType<SqlServerServerResource>());
        Assert.Equal("sqlserver", sqlServer.Name);

        var databases = sqlServer.Databases.OrderBy(d => d.Key).ToList();
        Assert.Equal(
            [
                new KeyValuePair<string, string>("catalogdb", "Microsoft.eShopOnWeb.CatalogDb"),
                new KeyValuePair<string, string>("identitydb", "Microsoft.eShopOnWeb.Identity"),
            ],
            databases);
    }

    [Fact]
    public async Task SqlServerContainerPersistsDataBetweenRuns()
    {
        await using var builder = await CreateAppHostBuilderAsync();

        var sqlServer = Assert.Single(builder.Resources.OfType<SqlServerServerResource>());

        var lifetime = Assert.Single(sqlServer.Annotations.OfType<ContainerLifetimeAnnotation>());
        Assert.Equal(ContainerLifetime.Persistent, lifetime.Lifetime);
        Assert.Contains(sqlServer.Annotations.OfType<ContainerMountAnnotation>(),
            mount => mount.Type == ContainerMountType.Volume && mount.Target == "/var/opt/mssql");
    }

    [Theory]
    [InlineData("web")]
    [InlineData("publicapi")]
    public async Task ProjectGetsConnectionStringsForBothDatabases(string projectName)
    {
        await using var builder = await CreateAppHostBuilderAsync();
        var project = GetProject(builder, projectName);

        // Publish mode renders references as expressions instead of resolving live endpoints.
        var configuration = await ExecutionConfigurationBuilder.Create(project)
            .WithEnvironmentVariablesConfig()
            .BuildAsync(new DistributedApplicationExecutionContext(DistributedApplicationOperation.Publish),
                NullLogger.Instance,
                TestContext.Current.CancellationToken);
        var environment = configuration.EnvironmentVariables.ToDictionary();

        Assert.Equal("{catalogdb.connectionString}", environment["ConnectionStrings__CatalogConnection"]);
        Assert.Equal("{identitydb.connectionString}", environment["ConnectionStrings__IdentityConnection"]);
    }

    [Fact]
    public async Task WebWaitsForBothDatabases()
    {
        await using var builder = await CreateAppHostBuilderAsync();

        var waitsFor = GetWaitDependencies(GetProject(builder, "web"));

        Assert.Contains("catalogdb", waitsFor);
        Assert.Contains("identitydb", waitsFor);
    }

    [Fact]
    public async Task PublicApiWaitsForWebToMigrateAndSeedTheDatabases()
    {
        // Both apps seed on startup with check-then-insert logic. If they start against an empty
        // database at the same time, the catalog data gets inserted twice.
        await using var builder = await CreateAppHostBuilderAsync();
        var web = GetProject(builder, "web");

        Assert.Contains("web", GetWaitDependencies(GetProject(builder, "publicapi")));
        Assert.Contains(web.Annotations.OfType<HealthCheckAnnotation>(), a => a.Key.StartsWith("web_", StringComparison.Ordinal));
    }

    private static List<string> GetWaitDependencies(ProjectResource project) =>
        project.Annotations.OfType<WaitAnnotation>().Select(a => a.Resource.Name).ToList();

    private static Task<IDistributedApplicationTestingBuilder> CreateAppHostBuilderAsync() =>
        DistributedApplicationTestingBuilder.CreateAsync<Projects.eShopWeb_AppHost>(TestContext.Current.CancellationToken);

    private static ProjectResource GetProject(IDistributedApplicationTestingBuilder builder, string name) =>
        Assert.Single(builder.Resources.OfType<ProjectResource>(), p => p.Name == name);
}
