using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.ApplicationCore.Constants;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.PublicApi.Extensions;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class CreateUserEndpoint(UserManager<ApplicationUser> userManager) : Endpoint<CreateUserRequest, CreateUserResponse>
{
    public override void Configure()
    {
        Post("api/users");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<CreateUserResponse>()
            .WithTags("UserManagementEndpoints")
        );
    }

    public override async Task HandleAsync(CreateUserRequest request, CancellationToken ct)
    {
        var response = new CreateUserResponse(request.CorrelationId());
        if (request is null || request.User is null || request.User.UserName is null)
        {
            await SendErrorsAsync(400, ct);
            return;
        }
        var existingUser = await userManager.FindByNameAsync(request.User.UserName);
        if (existingUser != null) {
            throw new DuplicateException($"User already exists.");
        };

        ApplicationUser newUser = new ApplicationUser();
        newUser.FromUserDto(request.User, copyId: false);

        var createUser = await userManager.CreateAsync(newUser);
        if (createUser.Succeeded)
        {
            var createdUser = await userManager.FindByNameAsync(request.User.UserName);
            await userManager.AddPasswordAsync(createdUser!, AuthorizationConstants.DEFAULT_PASSWORD);
            response.UserId = createdUser!.Id;
            await SendCreatedAtAsync<UserGetByIdEndpoint>(new { UserId = createdUser!.Id }, response, cancellation: ct);
        }
    }
}
