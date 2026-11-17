using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.ApplicationCore.Constants;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.Infrastructure;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.PublicApi.Extensions;
using Microsoft.Extensions.Hosting;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class CreateUserEndpoint(UserManager<ApplicationUser> userManager, IHostEnvironment environment) : Endpoint<CreateUserRequest, CreateUserResponse>
{
    public override void Configure()
    {
        Post("api/users");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
        {
            d.Produces<CreateUserResponse>(StatusCodes.Status201Created);
            d.ProducesProblemFE(StatusCodes.Status400BadRequest);
            d.Produces(StatusCodes.Status409Conflict);
            d.WithTags("UserManagementEndpoints");
        });
    }

    public override async Task HandleAsync(CreateUserRequest request, CancellationToken ct)
    {
        var response = new CreateUserResponse(request.CorrelationId());
        if (request is null || request.User is null || request.User.UserName is null)
        {
            await Send.ErrorsAsync(400, ct);
            return;
        }
        var existingUser = await userManager.FindByNameAsync(request.User.UserName);
        if (existingUser != null)
        {
            throw new DuplicateException($"User already exists.");
        }

        var user = request.User;
        // Only these fields are client settable. EmailConfirmed and PhoneNumberConfirmed are
        // ignored and start as false: they are set by the confirmation flows, never by the request.
        var newUser = new ApplicationUser
        {
            UserName = user.UserName,
            Email = user.Email,
            // Forms post "" for an empty field; store it as null, like PUT api/users.
            PhoneNumber = string.IsNullOrEmpty(user.PhoneNumber) ? null : user.PhoneNumber,
            LockoutEnd = user.LockoutEnd,
        };

        // Outside local development, users are created without a password and must set one themselves.
        // Creating the user and its password in one call means a rejected password leaves no user behind.
        var result = environment.IsLocalDevelopment()
            ? await userManager.CreateAsync(newUser, AuthorizationConstants.DEFAULT_PASSWORD)
            : await userManager.CreateAsync(newUser);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                AddError(error.Description, error.Code);
            }
            ThrowIfAnyErrors();
        }

        response.UserId = newUser.Id;
        await Send.CreatedAtAsync<UserGetByIdEndpoint>(new { UserId = newUser.Id }, response, cancellation: ct);
    }
}
