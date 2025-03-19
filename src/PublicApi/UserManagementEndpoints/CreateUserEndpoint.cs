using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.Infrastructure.Identity;

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
        Guard.Against.Null(request);
        Guard.Against.Null(request.User);
        Guard.Against.Null(request.User.UserName);
        var existingUser = await userManager.FindByNameAsync(request.User.UserName);
        if (existingUser != null) {
            throw new DuplicateException($"A user with the name {request.User.UserName} already exists");
        };
        var createUser = await userManager.CreateAsync(request.User);
        if (createUser.Succeeded)
        {
            var createdUser = await userManager.FindByNameAsync(request.User.UserName);
            response.User = createdUser!;
            await SendCreatedAtAsync<UserGetByIdEndpoint>(new { UserId = response.User.Id }, response, cancellation: ct);
        }
    }
}
