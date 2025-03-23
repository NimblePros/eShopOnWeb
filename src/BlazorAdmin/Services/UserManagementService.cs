using System.Threading.Tasks;
using BlazorAdmin.Interfaces;
using BlazorAdmin.Models;
using Microsoft.Extensions.Logging;

namespace BlazorAdmin.Services;

public class UserManagementService(HttpService httpService, ILogger<IUserManagementService> logger) : IUserManagementService
{
    public async Task<CreateUserResponse> Create(CreateUserRequest user)
    {
        var response = await httpService.HttpPost<CreateUserResponse>($"users", user);
        return response;
    }

    public async Task Delete(string userId)
    {
        await httpService.HttpDelete($"users/{userId}");
    }

    public async Task<GetUserResponse> Update(UpdateUserRequest user)
    {
        return await httpService.HttpPut<GetUserResponse>($"users", user);
    }

    public async Task<GetUserResponse> GetById(string userId)
    {
        return await httpService.HttpGet<GetUserResponse>($"users/{userId}");
    }

    public async Task<GetUserRolesResponse> GetRolesByUserId(string userId)
    {
        return await httpService.HttpGet<GetUserRolesResponse>($"users/{userId}/roles");
    }


    public async Task<GetUserResponse> GetByName(string userName) {
        return await httpService.HttpGet<GetUserResponse>($"users/name/{userName}");
    }

    public async Task<UserListResponse> List()
    {
        logger.LogInformation("Fetching users");
        return await httpService.HttpGet<UserListResponse>($"users");
    }

    public async Task SaveRolesForUser(SaveRolesForUserRequest request)
    {
        await httpService.HttpPut($"users/{request.UserId}/roles",request);
    }
}
