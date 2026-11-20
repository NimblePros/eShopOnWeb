namespace Microsoft.eShopWeb.Web.Configuration;

public static class ConfigureCookieSettings
{
    public const int ValidityMinutesPeriod = 60;
    public const string IdentifierCookieName = "EshopIdentifier";

    public static IServiceCollection AddCookieSettings(this IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //TODO need to check that.
            //options.CheckConsentNeeded = context => true;
            // Lax, not Strict: Strict would withhold the basket cookie when a shopper follows a link
            // from another site. Cookies that need Strict (sign-in, antiforgery) set it themselves.
            // See docs/decisions/2026-11-19-cookie-samesite-policy.md.
            options.MinimumSameSitePolicy = SameSiteMode.Lax;
        });
        services.ConfigureApplicationCookie(options =>
        {
            options.EventsType = typeof(RevokeAuthenticationEvents);
            options.ExpireTimeSpan = TimeSpan.FromMinutes(ValidityMinutesPeriod);
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            // Set properties on the existing builder rather than replacing it: a new CookieBuilder
            // defaults to HttpOnly = false, which would expose the auth ticket to JavaScript.
            options.Cookie.Name = IdentifierCookieName;
            options.Cookie.IsEssential = true; // required for auth to work without explicit user consent; adjust to suit your privacy policy
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        services.AddScoped<RevokeAuthenticationEvents>();

        return services;
    }
}
