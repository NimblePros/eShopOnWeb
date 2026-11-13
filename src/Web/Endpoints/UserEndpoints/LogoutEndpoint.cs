using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Antiforgery;
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
/// Requires an antiforgery token (from <see cref="GetAntiforgeryTokenEndpoint"/>) so other sites
/// can't sign the user out with a cross-site POST.
/// </summary>
public class LogoutEndpoint(SignInManager<ApplicationUser> signInManager,
    IAntiforgery antiforgery,
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
        if (!await antiforgery.IsRequestValidAsync(HttpContext))
        {
            logger.LogWarning("Logout rejected: missing or invalid antiforgery token.");
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
            return;
        }

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
