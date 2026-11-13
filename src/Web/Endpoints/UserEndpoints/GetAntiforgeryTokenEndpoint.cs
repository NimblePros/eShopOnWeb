using BlazorAdmin.Authorization;
using FastEndpoints;
using Microsoft.AspNetCore.Antiforgery;

namespace Microsoft.eShopWeb.Web.Endpoints.UserEndpoints;

/// <summary>
/// Issues an antiforgery request token for the current user and sets the matching cookie.
/// BlazorAdmin calls this before <see cref="LogoutEndpoint"/>. Other sites can't read the
/// response, so they can't get a token to forge a logout.
/// </summary>
public class GetAntiforgeryTokenEndpoint(IAntiforgery antiforgery)
    : EndpointWithoutRequest<AntiforgeryTokenResponse>
{
    public override void Configure()
    {
        Get("User/AntiforgeryToken");
        AllowAnonymous();
    }

    public override Task<AntiforgeryTokenResponse> ExecuteAsync(CancellationToken ct)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Task.FromResult(new AntiforgeryTokenResponse { Token = tokens.RequestToken! });
    }
}
