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

}
