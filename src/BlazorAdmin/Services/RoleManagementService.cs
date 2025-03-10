using System.Threading.Tasks;
using BlazorAdmin.Models;
using Microsoft.Extensions.Logging;

namespace BlazorAdmin.Services;

public class RoleManagementService
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
}
