using System.Threading.Tasks;
using BlazorAdmin.Interfaces;
using BlazorAdmin.Models;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.Logging;

namespace BlazorAdmin.Services;

public class UserManagementService(HttpService httpService, ILogger<IUserManagementService> logger) : IUserManagementService
{
    public async Task<CreateUserResponse> Create(ApplicationUser user)
    {
        var response = await httpService.HttpPost<CreateUserResponse>($"users", user);
        return response;
    }

    public async Task Delete(string userId)
    {
        await httpService.HttpDelete($"users/{userId}");
    }

    public async Task<ApplicationUser> Edit(ApplicationUser user)
    {
        return await httpService.HttpPut<ApplicationUser>($"users", user);
    }

    public async Task<ApplicationUser> GetById(string userId)
    {
        return await httpService.HttpGet<ApplicationUser>($"users/{userId}");
    }

    public async Task<UserListResponse> List()
    {
        logger.LogInformation("Fetching users");
        return await httpService.HttpGet<UserListResponse>($"users");
    }
}
