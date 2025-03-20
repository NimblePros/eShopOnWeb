using Microsoft.eShopWeb.Infrastructure.Identity;

namespace BlazorAdmin.Models;

public class CreateUserRequest
{
    public ApplicationUser User{  get; set; }

    public CreateUserRequest()
    {
        User = new ApplicationUser();
    }
}
