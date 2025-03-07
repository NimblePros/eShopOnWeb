using System.Net.Http.Headers;
using System.Security.Claims;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Newtonsoft.Json.Linq;

namespace Microsoft.eShopWeb.Web.Areas.Identity.Helpers;

public class GitHubClaimsHelper
{
    public static async Task OnOAuthCreatingTicket(OAuthCreatingTicketContext context)
    {
        // No JWT coming back from GitHub
        // Need to call the UserInformationEndpoint manually
        // And then build the claims from there.
        Guard.Against.Null(context);
        Guard.Against.Null(context.Identity);
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
        Guard.Against.Null(context.Identity);
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
}
