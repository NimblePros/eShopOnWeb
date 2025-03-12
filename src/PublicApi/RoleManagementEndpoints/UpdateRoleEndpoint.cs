using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class UpdateRoleEndpoint(RoleManager<IdentityRole> roleManager) : Endpoint<UpdateRoleRequest,Results<Ok<UpdateRoleResponse>,NotFound>>
{
    public override void Configure()
    {
        Put("api/roles");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<UpdateRoleResponse>()
             .WithTags("RoleManagementEndpoints"));
    }

    public override async Task<Results<Ok<UpdateRoleResponse>, NotFound>> ExecuteAsync(UpdateRoleRequest request, CancellationToken ct)
    {
        var response = new UpdateRoleResponse(request.CorrelationId());

        if (request is null)
        {
            return TypedResults.NotFound();
        }

        var existingRole = await roleManager.FindByIdAsync(request.Id);
        if (existingRole == null)
        {
            return TypedResults.NotFound();
        }

        existingRole.Name = request.Name;

        var result = await roleManager.UpdateAsync(existingRole);
        response.Role = (await roleManager.FindByIdAsync(existingRole.Id))!;
        return TypedResults.Ok(response);
    }
}
