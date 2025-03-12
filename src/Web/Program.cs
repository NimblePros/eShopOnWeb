using Ardalis.ListStartupServices;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web;
using Microsoft.eShopWeb.Web.Areas.Identity.Helpers;
using Microsoft.eShopWeb.Web.Configuration;
using Microsoft.eShopWeb.Web.Extensions;
using NimblePros.Metronome;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddAspireServiceDefaults();

builder.Services.AddDatabaseContexts(builder.Environment, builder.Configuration);

builder.Services.AddCookieSettings();
builder.Services.AddCookieAuthentication();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
           .AddDefaultUI()
           .AddEntityFrameworkStores<AppIdentityDbContext>()
           .AddDefaultTokenProviders();

var gitHubClientId = builder.Configuration["GitHub:ClientId"] ?? string.Empty;

if (!string.IsNullOrEmpty(gitHubClientId))
{
    builder.Services.AddAuthentication()
        .AddOAuth("GitHub", "GitHub", options =>
        {
            options.ClientId = gitHubClientId;
            options.ClientSecret = builder.Configuration["GitHub:ClientSecret"] ?? string.Empty;
            options.CallbackPath = "/signin-github";
            options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
            options.TokenEndpoint = "https://github.com/login/oauth/access_token";
            options.UserInformationEndpoint = "https://api.github.com/user";
            options.UsePkce = false; // PKCE not supported by GitHub       
            options.SaveTokens = true;
            options.ClaimsIssuer = "GitHub";
            options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
            {
                OnCreatingTicket = GitHubClaimsHelper.OnOAuthCreatingTicket
            };
        });
}

builder.Services.AddScoped<ITokenClaimsService, IdentityTokenClaimService>();
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddWebServices(builder.Configuration);

// Add memory cache services
builder.Services.AddMemoryCache();
builder.Services.AddRouting(options =>
{
    // Replace the type and the name used to refer to it with your own
    // IOutboundParameterTransformer implementation
    options.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer);
});

builder.Services.AddMvc(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(
             new SlugifyParameterTransformer()));

});
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Basket/Checkout");
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddCustomHealthChecks();

builder.Services.Configure<ServiceConfig>(config =>
{
    config.Services = new List<ServiceDescriptor>(builder.Services);
    config.Path = "/allservices";
});

builder.Services.AddBlazor(builder.Configuration);

builder.Services.AddMetronome();
builder.AddSeqEndpoint(connectionName: "seq");

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

app.Logger.LogInformation("App created...");

await app.SeedDatabaseAsync();

var catalogBaseUrl = builder.Configuration.GetValue(typeof(string), "CatalogBaseUrl") as string;
if (!string.IsNullOrEmpty(catalogBaseUrl))
{
    app.Use((context, next) =>
    {
        context.Request.PathBase = new PathString(catalogBaseUrl);
        return next();
    });
}


app.UseCustomHealthChecks();

app.UseTroubleshootingMiddlewares();

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserContextEnrichmentMiddleware>();
app.MapControllerRoute("default", "{controller:slugify=Home}/{action:slugify=Index}/{id?}");
app.MapRazorPages();
app.MapHealthChecks("home_page_health_check", new HealthCheckOptions { Predicate = check => check.Tags.Contains("homePageHealthCheck") });
app.MapHealthChecks("api_health_check", new HealthCheckOptions { Predicate = check => check.Tags.Contains("apiHealthCheck") });
//endpoints.MapBlazorHub("/admin");
app.MapFallbackToFile("index.html");

app.Logger.LogInformation("LAUNCHING");
app.Run();
