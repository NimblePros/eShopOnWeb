using System;
using System.Text;
using Azure.Identity;
using BlazorShared;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Constants;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.Infrastructure;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NSwag;

namespace Microsoft.eShopWeb.PublicApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddDatabaseContexts(this IServiceCollection services, IHostEnvironment environment, ConfigurationManager configuration)
    {
        if (environment.IsDevelopment() || environment.EnvironmentName == "Docker")
        {
            // Configure SQL Server (local/Docker)
            services.ConfigureLocalDatabaseContexts(configuration);
        }
        else
        {
            // Configure SQL Server (prod)
            var credential = new DefaultAzureCredential();
            configuration.AddAzureKeyVault(new Uri(configuration["AZURE_KEY_VAULT_ENDPOINT"] ?? ""), credential);

            services.AddDbContext<CatalogContext>((provider, options) =>
            {
                var connectionString = configuration[configuration["AZURE_SQL_CATALOG_CONNECTION_STRING_KEY"] ?? ""];
                options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure())
                    .AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>());
            });
            services.AddDbContext<AppIdentityDbContext>((provider, options) =>
            {
                var connectionString = configuration[configuration["AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY"] ?? ""];
                options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure())
                    .AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>());
            });
        }
    }

    public static void AddCustomServices(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));
        services.Configure<CatalogSettings>(configuration);

        var catalogSettings = configuration.Get<CatalogSettings>() ?? new CatalogSettings();
        services.AddSingleton<IUriComposer>(new UriComposer(catalogSettings));
        services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
        services.AddScoped<ITokenClaimsService, IdentityTokenClaimService>();
    }

    public static void AddJwtAuthentication(this IServiceCollection services)
    {
        var key = Encoding.ASCII.GetBytes(AuthorizationConstants.JWT_SECRET_KEY);

        services.AddAuthentication(config =>
        {
            config.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(config =>
        {
            config.RequireHttpsMetadata = false;
            config.SaveToken = true;
            config.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });
    }

    public static void AddCorsPolicy(this IServiceCollection services, string policyName, BaseUrlConfiguration baseUrlConfig)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: policyName,
                corsPolicyBuilder =>
                {
                    corsPolicyBuilder.WithOrigins(baseUrlConfig!.WebBase.Replace("host.docker.internal", "localhost").TrimEnd('/'));
                    corsPolicyBuilder.AllowAnyMethod();
                    corsPolicyBuilder.AllowAnyHeader();
                });
        });
    }

    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.SwaggerDocument(c =>
        {
            c.DocumentSettings = s =>
            {
                s.Title = "My API";
                s.Version = "v1";
                s.AddAuth("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });
            };
            c.EnableJWTBearerAuth = false;
            c.AutoTagPathSegmentIndex = 0;
            c.ShortSchemaNames = true;
        });
    }
}