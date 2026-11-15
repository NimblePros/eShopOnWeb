using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using AuthRoles = BlazorShared.Authorization.Constants.Roles;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class UpdateRoleEndpoint(RoleManager<IdentityRole> roleManager) : Endpoint<UpdateRoleRequest, Results<Ok<UpdateRoleResponse>, NotFound>>
{
    // Authorization checks depend on these names (Constants.Roles), so renaming them would break access control.
    private static readonly string[] BuiltInRoles = [AuthRoles.ADMINISTRATORS, AuthRoles.PRODUCT_MANAGERS];

    public override void Configure()
    {
        Put("api/roles");
        Roles(AuthRoles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
        {
            d.Produces<UpdateRoleResponse>();
            d.ProducesProblemFE(StatusCodes.Status400BadRequest);
            d.Produces(StatusCodes.Status404NotFound);
            d.Produces(StatusCodes.Status409Conflict);
            d.WithTags("RoleManagementEndpoints");
        });
    }

    public override async Task<Results<Ok<UpdateRoleResponse>, NotFound>> ExecuteAsync(UpdateRoleRequest request, CancellationToken ct)
    {
        var response = new UpdateRoleResponse(request.CorrelationId());

        var existingRole = await roleManager.FindByIdAsync(request.Id);
        if (existingRole == null)
        {
            return TypedResults.NotFound();
        }

        if (BuiltInRoles.Contains(existingRole.Name, StringComparer.OrdinalIgnoreCase))
        {
            ThrowError(r => r.Id, $"The built-in {existingRole.Name} role cannot be renamed.");
        }

        var roleWithSameName = await roleManager.FindByNameAsync(request.Name);
        if (roleWithSameName != null && roleWithSameName.Id != existingRole.Id)
        {
            throw new DuplicateException($"A role with name {request.Name} already exists");
        }

        existingRole.Name = request.Name;

        var result = await roleManager.UpdateAsync(existingRole);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                AddError(r => r.Name, error.Description, error.Code);
            }
            ThrowIfAnyErrors();
        }

        response.Role = (await roleManager.FindByIdAsync(existingRole.Id))!;
        return TypedResults.Ok(response);
    }
}
