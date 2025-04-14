using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorAdmin.Helpers;
using BlazorAdmin.Interfaces;
using BlazorAdmin.Models;
using Microsoft.AspNetCore.Identity;

namespace BlazorAdmin.Pages.RolePage;

public partial class List : BlazorComponent
{
    [Microsoft.AspNetCore.Components.Inject]
    public IRoleManagementService RoleManagementService { get; set; }

    private List<IdentityRole> _roles = [];
    private Create CreateComponent { get; set; }
    private Delete DeleteComponent { get; set; }
    private Edit EditComponent { get; set; }
    private Details DetailsComponent { get; set; }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var response = await RoleManagementService.List();
            _roles = response.Roles;

            CallRequestRefresh();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async void DetailsClick(string id, string name)
    {
        await DetailsComponent.Open(id, name);
    }

    private async Task CreateClick()
    {
        await CreateComponent.Open();
    }

    private async Task EditClick(string id)
    {
        await EditComponent.Open(id);
    }

    private async Task DeleteClick(string id)
    {
        await DeleteComponent.Open(id);
    }

    private async Task ReloadRoles()
    {
        var roleCall = await RoleManagementService.List();
        _roles = roleCall.Roles;
        StateHasChanged();
    }

}
