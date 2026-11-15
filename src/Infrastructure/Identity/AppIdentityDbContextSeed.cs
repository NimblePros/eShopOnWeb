using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Constants;

namespace Microsoft.eShopWeb.Infrastructure.Identity;

public class AppIdentityDbContextSeed
{
    public const string SEED_DEMO_USERS_CONFIG_NAME = "SeedDemoUsers";

    public static async Task SeedAsync(AppIdentityDbContext identityDbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, bool seedDemoUsers)
    {

        if (identityDbContext.Database.IsSqlServer())
        {
            identityDbContext.Database.Migrate();
        }

        await roleManager.CreateAsync(new IdentityRole(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS));
        await roleManager.CreateAsync(new IdentityRole(BlazorShared.Authorization.Constants.Roles.PRODUCT_MANAGERS));

        // Demo users share a publicly known password, so only create them when explicitly allowed
        if (!seedDemoUsers) return;

        var defaultUser = new ApplicationUser { UserName = BlazorShared.Authorization.Constants.DemoUsers.DEMO_USER, Email = BlazorShared.Authorization.Constants.DemoUsers.DEMO_USER };
        await userManager.CreateAsync(defaultUser, AuthorizationConstants.DEFAULT_PASSWORD);

        var productManager = new ApplicationUser { UserName = BlazorShared.Authorization.Constants.DemoUsers.PRODUCT_MANAGER, Email = BlazorShared.Authorization.Constants.DemoUsers.PRODUCT_MANAGER };
        await userManager.CreateAsync(productManager, AuthorizationConstants.DEFAULT_PASSWORD);
        productManager = await userManager.FindByNameAsync(productManager.UserName);
        if (productManager != null)
        {
            await userManager.AddToRoleAsync(productManager, BlazorShared.Authorization.Constants.Roles.PRODUCT_MANAGERS);
        }

        string adminUserName = AuthorizationConstants.DEFAULT_ADMIN_USERNAME;
        var adminUser = new ApplicationUser { UserName = adminUserName, Email = adminUserName };
        await userManager.CreateAsync(adminUser, AuthorizationConstants.DEFAULT_PASSWORD);
        adminUser = await userManager.FindByNameAsync(adminUserName);
        if (adminUser != null)
        {
            await userManager.AddToRoleAsync(adminUser, BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        }
    }
}
