using System.Net.Mime;
using Ardalis.ListStartupServices;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NimblePros.Metronome;

namespace Microsoft.eShopWeb.Web.Extensions;

public static class WebApplicationExtensions
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        app.Logger.LogInformation("Seeding Database...");

        using var scope = app.Services.CreateScope();
        var scopedProvider = scope.ServiceProvider;
        try
        {
            var catalogContext = scopedProvider.GetRequiredService<CatalogContext>();
            await CatalogContextSeed.SeedAsync(catalogContext, app.Logger);

            var userManager = scopedProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scopedProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var identityContext = scopedProvider.GetRequiredService<AppIdentityDbContext>();
            await AppIdentityDbContextSeed.SeedAsync(identityContext, userManager, roleManager);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred seeding the DB.");
        }
    }

    public static void UseCustomHealthChecks(this WebApplication app)
    {
        app.UseHealthChecks("/health",
            new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    var result = new
                    {
                        status = report.Status.ToString(),
                        errors = report.Entries.Select(e => new
                        {
                            key = e.Key,
                            value = Enum.GetName(typeof(HealthStatus), e.Value.Status)
                        })
                    }.ToJson();
                    context.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.Response.WriteAsync(result);
                }
            });
    }

    public static void UseTroubleshootingMiddlewares(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsDocker())
        {
            app.Logger.LogInformation("Adding Development middleware...");
            app.UseDeveloperExceptionPage();
            app.UseShowAllServicesMiddleware();
            app.UseMigrationsEndPoint();
            app.UseWebAssemblyDebugging();
            app.UseMetronomeLoggingMiddleware();
        }
        else
        {
            app.Logger.LogInformation("Adding non-Development middleware...");
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
    }
}
