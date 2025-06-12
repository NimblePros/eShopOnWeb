using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.PublicApi.Extensions;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class UserGetByUserNameEndpoint (UserManager<ApplicationUser> userManager) : Endpoint <GetByUserNameUserRequest, Results<Ok<GetUserResponse>,NotFound>>
{
    public override void Configure()
    {
        Get("api/users/name/{userName}");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<GetUserResponse>()
            .WithTags("UserManagementEndpoints"));
    }

    public override async Task<Results<Ok<GetUserResponse>, NotFound>> ExecuteAsync(GetByUserNameUserRequest request, CancellationToken ct)
    {
        var response = new GetUserResponse(request.CorrelationId());

        var userResponse = await userManager.FindByNameAsync(request.UserName);
        if (userResponse is null)
        {
            return TypedResults.NotFound();
        }
        response.User = userResponse.ToUserDto();
        return TypedResults.Ok(response);
    }
}
