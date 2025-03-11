using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Constants;

namespace Microsoft.eShopWeb.Infrastructure.Identity;

public class AppIdentityDbContextSeed
{
    public static async Task SeedAsync(AppIdentityDbContext identityDbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {

        if (identityDbContext.Database.IsSqlServer())
        {
            identityDbContext.Database.Migrate();
        }

        await roleManager.CreateAsync(new IdentityRole(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS));
        await roleManager.CreateAsync(new IdentityRole(BlazorShared.Authorization.Constants.Roles.PRODUCT_MANAGERS));

        var defaultUser = new ApplicationUser { UserName = "demouser@microsoft.com", Email = "demouser@microsoft.com" };
        await userManager.CreateAsync(defaultUser, AuthorizationConstants.DEFAULT_PASSWORD);

        var productManager = new ApplicationUser { UserName = "productmgr@microsoft.com", Email = "productmgr@microsoft.com" };
        await userManager.CreateAsync(productManager, AuthorizationConstants.DEFAULT_PASSWORD);
        productManager = await userManager.FindByNameAsync(productManager.UserName);
        if (productManager != null)
        {
            await userManager.AddToRoleAsync(productManager, BlazorShared.Authorization.Constants.Roles.PRODUCT_MANAGERS);
        }

        string adminUserName = "admin@microsoft.com";
        var adminUser = new ApplicationUser { UserName = adminUserName, Email = adminUserName };
        await userManager.CreateAsync(adminUser, AuthorizationConstants.DEFAULT_PASSWORD);
        adminUser = await userManager.FindByNameAsync(adminUserName);
        if (adminUser != null)
        {
            await userManager.AddToRoleAsync(adminUser, BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        }
    }
}
