using System;
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

public class UpdateUserEndpoint(UserManager<ApplicationUser> userManager) : Endpoint<UpdateUserRequest, Results<Ok<UpdateUserResponse>, NotFound>>
{
    public override void Configure()
    {
        Put("api/users");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
        {
            d.Produces<UpdateUserResponse>();
            d.ProducesProblemFE(StatusCodes.Status400BadRequest);
            d.Produces(StatusCodes.Status404NotFound);
            d.WithTags("UserManagementEndpoints");
        });
    }

    public override async Task<Results<Ok<UpdateUserResponse>, NotFound>> ExecuteAsync(UpdateUserRequest request, CancellationToken ct)
    {
        var response = new UpdateUserResponse(request.CorrelationId());

        if (request is null || request.User is null || request.User.Id is null)
        {
            return TypedResults.NotFound();
        }

        var existingUser = await userManager.FindByIdAsync(request.User.Id);
        if (existingUser is null)
        {
            return TypedResults.NotFound();
        }

        if (existingUser.IsDefaultAdmin())
        {
            ThrowError(r => r.User, "The default admin user cannot be changed.");
        }

        var changes = request.User;
        // Forms post "" for an empty field; treat it as null so an untouched field isn't a change.
        var phoneNumber = string.IsNullOrEmpty(changes.PhoneNumber) ? null : changes.PhoneNumber;
        var identityChanged = existingUser.UserName != changes.UserName || existingUser.Email != changes.Email;
        var phoneChanged = existingUser.PhoneNumber != phoneNumber;
        var lockoutChanged = existingUser.LockoutEnd != changes.LockoutEnd;

        // Only these fields are client editable. EmailConfirmed and PhoneNumberConfirmed are
        // ignored: they are set by the confirmation flows, never by the request.
        if (existingUser.Email != changes.Email)
        {
            existingUser.EmailConfirmed = false;
        }
        if (phoneChanged)
        {
            existingUser.PhoneNumberConfirmed = false;
        }
        existingUser.UserName = changes.UserName;
        existingUser.Email = changes.Email;
        existingUser.PhoneNumber = phoneNumber;
        existingUser.LockoutEnd = changes.LockoutEnd;

        // A new security stamp signs the user out of Web cookie sessions at the next stamp check.
        // It does not revoke JWTs already issued for the API. UpdateSecurityStampAsync validates,
        // normalizes and saves the user in one step, so a failure leaves nothing half applied.
        var result = identityChanged || phoneChanged || lockoutChanged
            ? await userManager.UpdateSecurityStampAsync(existingUser)
            : await userManager.UpdateAsync(existingUser);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                AddError(error.Description, error.Code);
            }
            ThrowIfAnyErrors();
        }

        response.User = existingUser.ToUserDto();
        return TypedResults.Ok(response);
    }
}
