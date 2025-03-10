using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorAdmin.Helpers;
using BlazorAdmin.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BlazorAdmin.Pages.RolePage;

public partial class List : BlazorComponent
{
    [Microsoft.AspNetCore.Components.Inject]
    public RoleManagementService RoleService { get; set; }

    private List<IdentityRole> roles = new List<IdentityRole>();
    private Create CreateComponent { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var response = await RoleService.List();
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
        var roleCall = await RoleService.List();
        roles = roleCall.Roles;
        StateHasChanged();
    }

}
