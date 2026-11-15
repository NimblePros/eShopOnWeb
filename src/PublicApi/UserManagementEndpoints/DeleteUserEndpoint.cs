using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.PublicApi.Extensions;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class DeleteUserEndpoint(UserManager<ApplicationUser> userManager) : Endpoint<DeleteUserRequest, Results<NoContent, NotFound, BadRequest>>
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

    public override async Task<Results<NoContent, NotFound, BadRequest>> ExecuteAsync(DeleteUserRequest request, CancellationToken ct)
    {
        var userToDelete = await userManager.FindByIdAsync(request.UserId);
        if (userToDelete is null || string.IsNullOrEmpty(userToDelete.UserName))
        {
            return TypedResults.NotFound();
        }
        if (userToDelete.IsDefaultAdmin())
        {
            return TypedResults.BadRequest();
        }

        await userManager.DeleteAsync(userToDelete);

        return TypedResults.NoContent();

    }
}
