using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Infrastructure;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureDatabaseContexts(this IServiceCollection services, IWebHostEnvironment environment, ConfigurationManager configuration)
    {
        if (environment.IsDevelopment() || environment.EnvironmentName == "Docker")
        {
            // Configure SQL Server (local)
            services.ConfigureLocalDatabaseContexts(configuration);
        }
        else
        {
            // Configure SQL Server (prod)
            var credential = new ChainedTokenCredential(new AzureDeveloperCliCredential(), new DefaultAzureCredential());
            configuration.AddAzureKeyVault(new Uri(configuration["AZURE_KEY_VAULT_ENDPOINT"] ?? ""), credential);

            services.AddDbContext<CatalogContext>(c =>
            {
                var connectionString = configuration[configuration["AZURE_SQL_CATALOG_CONNECTION_STRING_KEY"] ?? ""];
                c.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());
            });
            services.AddDbContext<AppIdentityDbContext>(options =>
            {
                var connectionString = configuration[configuration["AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY"] ?? ""];
                options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());
            });
        }
    }
}
