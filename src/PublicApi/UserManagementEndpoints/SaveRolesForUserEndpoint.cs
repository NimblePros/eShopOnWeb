using System;
using System.Collections.Generic;
using System.Linq;
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

public class SaveRolesForUserEndpoint(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    : Endpoint<SaveRolesForUserRequest, Results<Ok, NotFound>>
{
    public override void Configure()
    {
        Put("api/users/{userId}/roles");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
        {
            d.Produces(StatusCodes.Status200OK);
            d.ProducesProblemFE(StatusCodes.Status400BadRequest);
            d.Produces(StatusCodes.Status404NotFound);
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
            ThrowError(r => r.UserId, "The user has no user name and cannot be updated.");
        }
        if (userToUpdate.IsDefaultAdmin())
        {
            ThrowError(r => r.UserId, "The roles of the default admin user cannot be changed.");
        }

        var rolesToAdd = request.RolesToAdd.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var rolesToRemove = request.RolesToRemove.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        // Validate everything up front so a bad request never leaves the user partly updated.
        foreach (var role in rolesToAdd.Intersect(rolesToRemove, StringComparer.OrdinalIgnoreCase))
        {
            AddError(r => r.RolesToAdd, $"Role {role} cannot be both added and removed.");
        }
        foreach (var role in rolesToAdd.Concat(rolesToRemove).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                AddError(r => r.RolesToAdd, $"Role {role} does not exist.");
            }
        }
        ThrowIfAnyErrors();

        var currentRoles = await userManager.GetRolesAsync(userToUpdate);
        foreach (var role in rolesToAdd.Where(r => currentRoles.Contains(r, StringComparer.OrdinalIgnoreCase)))
        {
            AddError(r => r.RolesToAdd, $"User is already in role {role}.");
        }
        foreach (var role in rolesToRemove.Where(r => !currentRoles.Contains(r, StringComparer.OrdinalIgnoreCase)))
        {
            AddError(r => r.RolesToRemove, $"User is not in role {role}.");
        }
        ThrowIfAnyErrors();

        if (rolesToAdd.Count > 0)
        {
            ThrowIfFailed(await userManager.AddToRolesAsync(userToUpdate, rolesToAdd));
        }
        if (rolesToRemove.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(userToUpdate, rolesToRemove);
            if (!removeResult.Succeeded && rolesToAdd.Count > 0)
            {
                // Undo the add so the user is not left partly updated.
                await userManager.RemoveFromRolesAsync(userToUpdate, rolesToAdd);
            }
            ThrowIfFailed(removeResult);
        }

        return TypedResults.Ok();
    }

    private void ThrowIfFailed(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        foreach (var error in result.Errors)
        {
            AddError(error.Description, error.Code);
        }
        ThrowIfAnyErrors();
    }
}
