using System.Security.Claims;
using BlazorAdmin.Authorization;
using FastEndpoints;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.Web.Endpoints.UserEndpoints;

/// <summary>
/// Returns the signed-in user's claims and an API token. BlazorAdmin calls this to build its
/// authentication state. Anonymous callers get <see cref="UserInfo.Anonymous"/>.
/// </summary>
public class GetCurrentUserEndpoint(ITokenClaimsService tokenClaimsService)
    : EndpointWithoutRequest<UserInfo>
{
    public override void Configure()
    {
        Get("User");
        AllowAnonymous();
    }

    public override async Task<UserInfo> ExecuteAsync(CancellationToken ct) =>
        await CreateUserInfoAsync(User);

    private async Task<UserInfo> CreateUserInfoAsync(ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal.Identity?.Name == null || !claimsPrincipal.Identity.IsAuthenticated)
        {
            return UserInfo.Anonymous;
        }

        var userInfo = new UserInfo
        {
            IsAuthenticated = true
        };

        if (claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            userInfo.NameClaimType = claimsIdentity.NameClaimType;
            userInfo.RoleClaimType = claimsIdentity.RoleClaimType;
        }
        else
        {
            userInfo.NameClaimType = "name";
            userInfo.RoleClaimType = "role";
        }

        if (claimsPrincipal.Claims.Any())
        {
            var claims = new List<ClaimValue>();
            var nameClaims = claimsPrincipal.FindAll(userInfo.NameClaimType);
            foreach (var claim in nameClaims)
            {
                claims.Add(new ClaimValue(userInfo.NameClaimType, claim.Value));
            }

            foreach (var claim in claimsPrincipal.Claims.Except(nameClaims))
            {
                claims.Add(new ClaimValue(claim.Type, claim.Value));
            }

            userInfo.Claims = claims;
        }

        userInfo.Token = await tokenClaimsService.GetTokenAsync(claimsPrincipal.Identity.Name);

        return userInfo;
    }
}
