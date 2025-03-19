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

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class DeleteUserEndpoint(UserManager<ApplicationUser> userManager) : Endpoint<DeleteUserRequest, Results<NoContent, NotFound>>
{
    public override void Configure()
    {
        Delete("api/users/{userId}");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
        {
            d.Produces(StatusCodes.Status204NoContent);
            d.Produces(StatusCodes.Status404NotFound);
            d.Produces(StatusCodes.Status409Conflict);
            d.WithTags("UserManagementEndpoints");
        }
        );
    }

    public override async Task<Results<NoContent, NotFound>> ExecuteAsync(DeleteUserRequest request, CancellationToken ct)
    {
        var userToDelete = await userManager.FindByIdAsync(request.UserId);
        if (userToDelete is null)
        {
            return TypedResults.NotFound();
        }

        if (string.IsNullOrEmpty(userToDelete.UserName))
        {
            throw new System.Exception("Unknown user to delete");
        }
        if (userToDelete.UserName == "admin@microsoft.com")
        {
            throw new AdminProtectionException();
        }

        await userManager.DeleteAsync(userToDelete);

        return TypedResults.NoContent();

    }
}
