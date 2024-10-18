using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.PublicApi.AuthEndpoints;

/// <summary>
/// Authenticates a user
/// </summary>
public class AuthenticateEndpoint(SignInManager<ApplicationUser> signInManager,
        ITokenClaimsService tokenClaimsService)
    : Endpoint<AuthenticateRequest, AuthenticateResponse>
{
    public override void Configure()
    {
        Post("api/authenticate");
        AllowAnonymous();
        Description(d =>
            d.WithSummary("Authenticates a user")
             .WithDescription("Authenticates a user")
             .WithName("auth.authenticate")
             .WithTags("AuthEndpoints"));
    }

    public override async Task<AuthenticateResponse> ExecuteAsync(AuthenticateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new AuthenticateResponse(request.CorrelationId());

        // This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
        //var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        var result = await signInManager.PasswordSignInAsync(request.Username, request.Password, false, true);

        response.Result = result.Succeeded;
        response.IsLockedOut = result.IsLockedOut;
        response.IsNotAllowed = result.IsNotAllowed;
        response.RequiresTwoFactor = result.RequiresTwoFactor;
        response.Username = request.Username;

        if (result.Succeeded)
        {
            response.Token = await tokenClaimsService.GetTokenAsync(request.Username);
        }

        return response;
    }
}
