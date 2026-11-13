using Mediator;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.Services;

namespace Microsoft.eShopWeb.Web.Configuration;

public static class ConfigureWebServices
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Mediator support for the services
        services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
        services.AddScoped<IBasketViewModelService, BasketViewModelService>();
        services.AddScoped<IEmailVerificationService, EmailVerificationService>();
        services.AddScoped<CatalogViewModelService>();
        services.AddScoped<ICatalogItemViewModelService, CatalogItemViewModelService>();
        services.Configure<CatalogSettings>(configuration);
        services.AddScoped<ICatalogViewModelService, CachedCatalogViewModelService>();

        return services;
    }
}
