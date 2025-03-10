using System.Threading.Tasks;
using BlazorAdmin.Interfaces;
using BlazorAdmin.Models;
using BlazorShared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BlazorAdmin.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly HttpService _httpService;
    private readonly ILogger<RoleManagementService> _logger;

    public RoleManagementService(HttpService httpService, ILogger<RoleManagementService> logger)
    {
        _httpService = httpService;
        _logger = logger;
    }

    public async Task<RoleListResponse> List(){
        _logger.LogInformation("Fetching roles");
        var response = await _httpService.HttpGet<RoleListResponse>($"roles");
        return response;
    }

    public async Task<CreateRoleResponse> Create(CreateRoleRequest newRole)
    {
        var response = await _httpService.HttpPost<CreateRoleResponse>($"roles", newRole);
        return response;
    }

    public Task<IdentityRole> Edit(IdentityRole role)
    {
        throw new System.NotImplementedException();
    }

    public async Task<string> Delete(string id)
    {
        return (await _httpService.HttpDelete<DeleteRoleResponse>($"roles", id)).Status;
    }

    public async Task<GetByIdRoleResponse> GetById(string id)
    {
        var roleById = await _httpService.HttpGet<GetByIdRoleResponse>($"roles/{id}");
        return roleById;
    }
}
