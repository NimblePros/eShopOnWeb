using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Microsoft.eShopWeb.Web.Areas.Identity.Helpers;

public class GitHubClaimsHelper
{
    public static async Task OnOAuthCreatingTicket(OAuthCreatingTicketContext context)
    {
        // No JWT coming back from GitHub
        // Need to call the UserInformationEndpoint manually
        // And then build the claims from there.
        if (context.Identity.IsAuthenticated)
        {
            // Store the tokens
            var tokens = context.Properties.GetTokens().ToList();
            context.Properties.StoreTokens(tokens);
            // Get the GitHub user
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            var user = JObject.Parse(await response.Content.ReadAsStringAsync());
            AddClaims(context, user);
        }
    }
    private static void AddClaims(OAuthCreatingTicketContext context, JObject user)
    {
        var identifier = user.Value<string>("id");
        if (!string.IsNullOrEmpty(identifier))
        {
            context.Identity.AddClaim(new Claim(
                ClaimTypes.NameIdentifier, identifier,
                ClaimValueTypes.String, context.Options.ClaimsIssuer));
        }

        var userName = user.Value<string>("login");
        if (!string.IsNullOrEmpty(userName))
        {
            context.Identity.AddClaim(new Claim(
                ClaimsIdentity.DefaultNameClaimType, userName,
                ClaimValueTypes.String, context.Options.ClaimsIssuer));
        }

        var name = user.Value<string>("name");
        if (!string.IsNullOrEmpty(name))
        {
            context.Identity.AddClaim(new Claim(
                "urn:github:name", name,
                ClaimValueTypes.String, context.Options.ClaimsIssuer));
        }

        var link = user.Value<string>("url");
        if (!string.IsNullOrEmpty(link))
        {
            context.Identity.AddClaim(new Claim(
                "urn:github:url", link,
                ClaimValueTypes.String, context.Options.ClaimsIssuer));
        }
    }

    public static async Task<bool> SaveClaimsAsync(Dictionary<string, string> claimsToSync, ExternalLoginInfo info, ApplicationUser user, UserManager<ApplicationUser> userManager)
    {
        var refreshSignIn = false;
        var userClaims = await userManager.GetClaimsAsync(user);

        foreach (var addedClaim in claimsToSync)
        {
            var userClaim = userClaims
                .FirstOrDefault(c => c.Type == addedClaim.Key);

            var externalClaim = info.Principal.FindFirst(addedClaim.Key);

            if (userClaim == null)
            {
                await userManager.AddClaimAsync(user,
                    new Claim(addedClaim.Key, externalClaim.Value));
                refreshSignIn = true;
            }
            else if (userClaim.Value != externalClaim.Value)
            {
                await userManager
                    .ReplaceClaimAsync(user, userClaim, externalClaim);
                refreshSignIn = true;
            }

        }
        return refreshSignIn;
    }
}
