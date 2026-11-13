using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.eShopWeb.Web.Endpoints.UserEndpoints;

/// <summary>
/// Signs the current user out. BlazorAdmin calls this from its logout page. The identity cookie
/// key is cached so <see cref="RevokeAuthenticationEvents"/> rejects that cookie if it is replayed.
/// </summary>
public class LogoutEndpoint(SignInManager<ApplicationUser> signInManager,
    ILogger<LogoutEndpoint> logger,
    IMemoryCache cache)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("User/Logout");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await signInManager.SignOutAsync();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var userId = User.FindFirst(ClaimTypes.Name);
        if (userId != null)
        {
            var identityKey = HttpContext.Request.Cookies[ConfigureCookieSettings.IdentifierCookieName];
            cache.Set($"{userId.Value}:{identityKey}", identityKey, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(ConfigureCookieSettings.ValidityMinutesPeriod)
            });
        }

        logger.LogInformation("User logged out.");
        await Send.OkAsync(cancellation: ct);
    }
}
