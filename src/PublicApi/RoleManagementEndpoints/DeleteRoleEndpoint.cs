using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using FastEndpoints;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading;
using Microsoft.eShopWeb.Infrastructure.Identity;
using System.Linq;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class DeleteRoleEndpoint(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager) : Endpoint <DeleteRoleRequest, Results<Ok<DeleteRoleResponse>, NotFound>>
{
    public override void Configure()
    {
        Delete("api/roles/{roleId}");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<DeleteRoleResponse>()
            .WithTags("RoleManagementEndpoints")
        );
    }

    public override async Task<Results<Ok<DeleteRoleResponse>,NotFound>> ExecuteAsync(DeleteRoleRequest request, CancellationToken ct)
    {
        var response = new DeleteRoleResponse(request.CorrelationId());
        var roleToDelete = await roleManager.FindByIdAsync(request.RoleId);
        if (roleToDelete is null)
        {
            return TypedResults.NotFound();
        }

        // Without this, the RoleManager will delete the role and treat it as a cascading delete.
        // If we accidentally deleted an important role, that would not be a good day.
        var usersWithRole = await userManager.GetUsersInRoleAsync(roleToDelete.Name);
        if (usersWithRole.Any())
        {
            throw new RoleStillAssignedException("This role is in use and cannot be deleted.");
        }

        await roleManager.DeleteAsync(roleToDelete);

        return TypedResults.Ok(response);

    }
}
