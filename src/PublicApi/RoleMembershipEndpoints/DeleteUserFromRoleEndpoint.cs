using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.Infrastructure.Identity;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace Microsoft.eShopWeb.PublicApi.RoleMembershipEndpoints;

public class DeleteUserFromRoleEndpoint(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager) : Endpoint<DeleteUserFromRoleRequest, Results<Ok<DeleteUserFromRoleResponse>, NotFound>>
{
    public override void Configure()
    {
        Delete("api/roles/{roleId}/members/{userId}");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<DeleteUserFromRoleRequest>()
            .WithTags("RoleManagementEndpoints")
        );
    }

    public override async Task<Results<Ok<DeleteUserFromRoleResponse>, NotFound>> ExecuteAsync(DeleteUserFromRoleRequest request, CancellationToken ct)
    {
        var response = new DeleteUserFromRoleResponse(request.CorrelationId());
        var userToUpdate = await userManager.FindByIdAsync(request.UserId);
        if (userToUpdate is null)
        {
            return TypedResults.NotFound();
        }

        var roleToUpdate = await roleManager.FindByIdAsync(request.RoleId);
        if (roleToUpdate is null || roleToUpdate.Name is null)
        {
            return TypedResults.NotFound();
        }

        await userManager.RemoveFromRoleAsync(userToUpdate, roleToUpdate.Name);
        
        return TypedResults.Ok(response);

    }
}
