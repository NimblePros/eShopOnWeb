using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorAdmin.Helpers;
using BlazorShared.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BlazorAdmin.Pages.RolePage;

public partial class List : BlazorComponent
{
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
          
        }

        await base.OnAfterRenderAsync(firstRender);
    }

}
