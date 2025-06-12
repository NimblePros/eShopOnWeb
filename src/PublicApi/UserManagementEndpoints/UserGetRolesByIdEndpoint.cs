using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class UserGetRolesByIdEndpoint (UserManager<ApplicationUser> userManager) : Endpoint <GetRolesByUserIdRequest, Results<Ok<GetUserRolesResponse>,NotFound>>
{
    public override void Configure()
    {
        Get("api/users/{userId}/roles");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<GetUserResponse>()
            .WithTags("UserManagementEndpoints"));
    }

    public override async Task<Results<Ok<GetUserRolesResponse>, NotFound>> ExecuteAsync(GetRolesByUserIdRequest request, CancellationToken ct)
    {
        var response = new GetUserRolesResponse(request.CorrelationId());

        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        var rolesForUser = await userManager.GetRolesAsync(user);
        if (rolesForUser is null)
        {
            return TypedResults.NotFound();
        }
        
        response.Roles = [.. rolesForUser];
        return TypedResults.Ok(response);
    }
}
