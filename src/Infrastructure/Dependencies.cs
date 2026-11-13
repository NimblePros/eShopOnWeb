using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NimblePros.Metronome;

namespace Microsoft.eShopWeb.Infrastructure;

public static class Dependencies
{
    public const string CATALOG_CONNECTION_NAME = "CatalogConnection";
    public const string IDENTITY_CONNECTION_NAME = "IdentityConnection";

    public static void ConfigureLocalDatabaseContexts(this IServiceCollection services, IConfiguration configuration)
    {
        bool useOnlyInMemoryDatabase = false;
        if (configuration["UseOnlyInMemoryDatabase"] != null)
        {
            useOnlyInMemoryDatabase = bool.Parse(configuration["UseOnlyInMemoryDatabase"]!);
        }

        if (useOnlyInMemoryDatabase)
        {
            services.AddDbContext<CatalogContext>((provider, options) =>
               options.UseInMemoryDatabase("Catalog"));

            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseInMemoryDatabase("Identity"));
        }
        else
        {
            // Use a real SQL Server database. Running the eShopWeb.AppHost (.NET Aspire) project
            // starts SQL Server in a container and supplies these connection strings.
            services.AddDbContext<CatalogContext>((provider, options) =>
            {
                string? catalogConnectionString = GetSqlConnectionString(configuration, CATALOG_CONNECTION_NAME);

                // why doesn't this extension method work?
                // see: https://github.com/NimblePros/Metronome/blob/main/src/NimblePros.Metronome/ServiceRegistrationExtensions.cs#L24
                //options.UseSqlServer(catalogConnectionString).AddMetronomeDbTracking(provider); ;

                // but this works
                options.UseSqlServer(catalogConnectionString)
                    .AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>());
            });

            // Add Identity DbContext
            services.AddDbContext<AppIdentityDbContext>((provider, options) =>
            {
                string? identityConnectionString = GetSqlConnectionString(configuration, IDENTITY_CONNECTION_NAME);
                options.UseSqlServer(identityConnectionString);
                // not sure why AddMetronomeDbTracking extension method doesn't work here
                options.AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>());
            });
        }
    }

    // Resolved when a DbContext is first created, not at registration, so tests that swap in
    // their own DbContextOptions never need a connection string. EF design-time tools
    // (e.g. "dotnet ef migrations add") don't connect, so they don't need one either.
    private static string? GetSqlConnectionString(IConfiguration configuration, string name)
    {
        string? connectionString = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString) && !EF.IsDesignTime)
        {
            throw new InvalidOperationException(
                $"Connection string '{name}' is not configured. Run the solution through the eShopWeb.AppHost " +
                "(.NET Aspire) project, which starts SQL Server in a container and supplies it, set " +
                $"'ConnectionStrings:{name}' yourself (for example with user secrets), or set " +
                "'UseOnlyInMemoryDatabase' to true.");
        }

        return connectionString;
    }

    public static void AddJwtSettings(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.CONFIG_NAME))
            .PostConfigure(settings =>
            {
                // Lets the sample run locally with no setup. JwtSettingsValidator rejects this key outside local development.
                if (string.IsNullOrWhiteSpace(settings.SigningKey) && environment.IsLocalDevelopment())
                {
                    settings.SigningKey = JwtSettings.DEVELOPMENT_SIGNING_KEY;
                }
            })
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<JwtSettings>, JwtSettingsValidator>();
    }
}
