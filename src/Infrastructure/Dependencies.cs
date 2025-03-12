using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NimblePros.Metronome;

namespace Microsoft.eShopWeb.Infrastructure;

public static class Dependencies
{
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
            // use real database
            // Requires LocalDB which can be installed with SQL Server Express 2016
            // https://www.microsoft.com/en-us/download/details.aspx?id=54284
            string catalogConnectionString = configuration.GetConnectionString("CatalogConnection")!;
            services.AddDbContext<CatalogContext>((provider, options) =>
            {
                // why doesn't this extension method work?
                // see: https://github.com/NimblePros/Metronome/blob/main/src/NimblePros.Metronome/ServiceRegistrationExtensions.cs#L24
                //options.UseSqlServer(catalogConnectionString).AddMetronomeDbTracking(provider); ;

                // but this works
                options.UseSqlServer(catalogConnectionString)
                    .AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>());
            });

            // Add Identity DbContext
            string identityConnectionString = configuration.GetConnectionString("IdentityConnection")!;
            services.AddDbContext<AppIdentityDbContext>((provider, options) =>
            {
                options.UseSqlServer(identityConnectionString);
                // not sure why AddMetronomeDbTracking extension method doesn't work here
                options.AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>());
            });
        }
    }
}
