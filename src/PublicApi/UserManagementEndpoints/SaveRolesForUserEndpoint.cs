using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class SaveRolesForUserEndpoint(UserManager<ApplicationUser> userManager) : Endpoint<SaveRolesForUserRequest, Results<Ok, NotFound>>
{
    public override void Configure()
    {
        Put("api/users/{userId}/roles");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
        {
            d.Produces(StatusCodes.Status200OK);
            d.Produces(StatusCodes.Status404NotFound);
            d.Produces(StatusCodes.Status409Conflict);
            d.WithTags("UserManagementEndpoints");
        }
        );
    }

    public override async Task<Results<Ok, NotFound>> ExecuteAsync(SaveRolesForUserRequest request, CancellationToken ct)
    {
        var userToUpdate = await userManager.FindByIdAsync(request.UserId);
        if (userToUpdate is null)
        {
            return TypedResults.NotFound();
        }

        if (string.IsNullOrEmpty(userToUpdate.UserName))
        {
            throw new System.Exception("Unknown user to update");
        }
        if (userToUpdate.UserName == "admin@microsoft.com")
        {
            throw new AdminProtectionException();
        }

        if (request.RolesToAdd.Any())
            await userManager.AddToRolesAsync(userToUpdate,request.RolesToAdd);
        if (request.RolesToRemove.Any())
            await userManager.RemoveFromRolesAsync(userToUpdate,request.RolesToRemove);

        return TypedResults.Ok();

    }
}
