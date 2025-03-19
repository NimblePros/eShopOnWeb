using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorAdmin.Helpers;
using BlazorAdmin.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.Logging;

namespace BlazorAdmin.Pages.UserPage;

public partial class List : BlazorComponent
{
    [Microsoft.AspNetCore.Components.Inject]
    public IUserManagementService UserManagementService{ get; set; }
    [Microsoft.AspNetCore.Components.Inject]
    ILogger<List> Logger { get; set; }

    private List<ApplicationUser> _users = [];
    private Create CreateComponent { get; set; }
    private Delete DeleteComponent { get; set; }
    private Edit EditComponent { get; set; }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var response = await UserManagementService.List();
            _users = response.Users;

            CallRequestRefresh();
        }

        await base.OnAfterRenderAsync(firstRender);
    }


    private async Task CreateClick()
    {
        Logger.LogInformation($"Display the user confirmation dialog");
        await CreateComponent.Open();
    }

    private async Task EditClick(string id)
    {
        Logger.LogInformation($"Edit User {id}");
        await EditComponent.Open(id);
    }

    private async Task DeleteClick(string id, string userName)
    {
        Logger.LogInformation($"Displaying delete confirmation for User {id}");
        await DeleteComponent.Open(id, userName);
    }

    private async Task ReloadUsers()
    {
        var usersCall = await UserManagementService.List();
        _users = usersCall.Users;
        StateHasChanged();
    }

}
