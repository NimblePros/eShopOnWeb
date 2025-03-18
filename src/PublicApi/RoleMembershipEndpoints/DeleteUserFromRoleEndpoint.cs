using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.PublicApi.RoleMembershipEndpoints;

public class DeleteUserFromRoleEndpoint(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager) : Endpoint<DeleteUserFromRoleRequest, Results<NoContent, NotFound>>
{
    public override void Configure()
    {
        Delete("api/roles/{RoleId}/members/{UserId}");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            {
                d.Produces(StatusCodes.Status204NoContent);
                d.Produces(StatusCodes.Status404NotFound);
                d.WithTags("RoleManagementEndpoints");
            }
        );
    }

    public override async Task<Results<NoContent, NotFound>> ExecuteAsync(DeleteUserFromRoleRequest request, CancellationToken ct)
    {
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
        
        return TypedResults.NoContent();

    }
}
