using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class UpdateRoleEndpoint(UserManager<ApplicationUser> userManager) : Endpoint<UpdateUserRequest,Results<Ok<UpdateUserResponse>,NotFound>>
{
    public override void Configure()
    {
        Put("api/users");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<UpdateUserResponse>()
             .WithTags("UserManagementEndpoints"));
    }

    public override async Task<Results<Ok<UpdateUserResponse>, NotFound>> ExecuteAsync(UpdateUserRequest request, CancellationToken ct)
    {
        var response = new UpdateUserResponse(request.CorrelationId());

        if (request is null)
        {
            return TypedResults.NotFound();
        }

        var existingUser = await userManager.FindByIdAsync(request.UserToUpdate.Id);
        if (existingUser == null)
        {
            return TypedResults.NotFound();
        }
        // TODO: Update fields

        var result = await userManager.UpdateAsync(existingUser);
        response.User = (await userManager.FindByIdAsync(existingUser.Id))!;
        return TypedResults.Ok(response);
    }
}
