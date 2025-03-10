using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorAdmin.Helpers;
using BlazorAdmin.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BlazorAdmin.Pages.RolePage;

public partial class List : BlazorComponent
{
    [Microsoft.AspNetCore.Components.Inject]
    public IRoleManagementService RoleManagementService { get; set; }

    private List<IdentityRole> roles = new List<IdentityRole>();
    private Create CreateComponent { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var response = await RoleManagementService.List();
            roles = response.Roles;

            CallRequestRefresh();
        }

        await base.OnAfterRenderAsync(firstRender);
    }
    private async Task CreateClick()
    {
        await CreateComponent.Open();
    }

    private async Task ReloadRoles()
    {
        var roleCall = await RoleManagementService.List();
        roles = roleCall.Roles;
        StateHasChanged();
    }

}
