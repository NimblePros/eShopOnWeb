using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class CreateRoleEndpoint(RoleManager<IdentityRole> roleManager) : Endpoint<CreateRoleRequest,CreateRoleResponse>
{
    public override void Configure()
    {
        Post("api/roles");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<CreateRoleResponse>()
            .WithTags("RoleManagementEndpoints")
        );
    }

    public override async Task HandleAsync(CreateRoleRequest request, CancellationToken ct)
    {
        var response = new CreateRoleResponse(request.CorrelationId());
        var existingRole = await roleManager.FindByNameAsync(request.Name);
        if (existingRole != null) {
            throw new DuplicateException($"A role with name {request.Name} already exists");
        }
        var newRole = new IdentityRole(request.Name);
        var createRole = await roleManager.CreateAsync(newRole);
        if (createRole.Succeeded)
        {
            var responseRole = await roleManager.FindByNameAsync(request.Name);
            response.Role = responseRole!;
            await SendCreatedAtAsync<RoleGetByIdEndpoint>(new { RoleId = response.Role.Id }, response, cancellation: ct);
        }
    }
}
