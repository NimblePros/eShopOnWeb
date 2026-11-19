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

        // Only these fields are client settable. EmailConfirmed, PhoneNumberConfirmed and
        // TwoFactorEnabled are ignored: they are set by the confirmation and 2FA flows.
        var newUser = new ApplicationUser
        {
            UserName = request.User.UserName,
            Email = request.User.Email,
            // Forms post "" for an empty field; store it as no phone number.
            PhoneNumber = string.IsNullOrEmpty(request.User.PhoneNumber) ? null : request.User.PhoneNumber,
            LockoutEnd = request.User.LockoutEnd,
        };

        var createUser = await userManager.CreateAsync(newUser);
        if (!createUser.Succeeded)
        {
            foreach (var error in createUser.Errors)
            {
                AddError(error.Description, error.Code);
            }
            ThrowIfAnyErrors();
        }

        // Outside local development, users are created without a password and must set one themselves
        if (environment.IsLocalDevelopment())
        {
            await userManager.AddPasswordAsync(newUser, AuthorizationConstants.DEFAULT_PASSWORD);
        }
        response.UserId = newUser.Id;
        await Send.CreatedAtAsync<UserGetByIdEndpoint>(new { UserId = newUser.Id }, response, cancellation: ct);
    }
}
