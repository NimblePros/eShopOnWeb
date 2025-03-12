using Azure.Identity;
using BlazorAdmin;
using BlazorAdmin.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Infrastructure;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Configuration;
using Microsoft.eShopWeb.Web.HealthChecks;

namespace Microsoft.eShopWeb.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddDatabaseContexts(this IServiceCollection services, IWebHostEnvironment environment, ConfigurationManager configuration)
    {
        if (environment.IsDevelopment() || environment.IsDocker())
        {
            // Configure SQL Server (local)
            services.ConfigureLocalDatabaseContexts(configuration);
        }
        else
        {
            // Configure SQL Server (prod)
            var credential = new ChainedTokenCredential(new AzureDeveloperCliCredential(), new DefaultAzureCredential());
            configuration.AddAzureKeyVault(new Uri(configuration["AZURE_KEY_VAULT_ENDPOINT"] ?? ""), credential);

            services.AddDbContext<CatalogContext>((provider, options) =>
            {
                var connectionString = configuration[configuration["AZURE_SQL_CATALOG_CONNECTION_STRING_KEY"] ?? ""];
                options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure())
                .AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>());
            });
            services.AddDbContext<AppIdentityDbContext>((provider,options) =>
            {
                var connectionString = configuration[configuration["AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY"] ?? ""];
                options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure())
                                .AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>());
            });
        }
    }

    public static void AddCookieAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });
    }

    public static void AddCustomHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<ApiHealthCheck>("api_health_check", tags: new[] { "apiHealthCheck" })
            .AddCheck<HomePageHealthCheck>("home_page_health_check", tags: new[] { "homePageHealthCheck" });
    }

    public static void AddBlazor(this IServiceCollection services, ConfigurationManager configuration)
    {
        var configSection = configuration.GetRequiredSection(BaseUrlConfiguration.CONFIG_NAME);
        services.Configure<BaseUrlConfiguration>(configSection);

        // Blazor Admin Required Services for Prerendering
        services.AddScoped<HttpClient>(s => new HttpClient
        {
            BaseAddress = new Uri("https+http://blazoradmin")
        });

        // add blazor services
        services.AddBlazoredLocalStorage();
        services.AddServerSideBlazor();
        services.AddScoped<ToastService>();
        services.AddScoped<HttpService>();
        services.AddBlazorServices();
    }
}
