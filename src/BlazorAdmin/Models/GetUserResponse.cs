using Microsoft.eShopWeb.Infrastructure.Identity;

namespace BlazorAdmin.Models;

public class GetUserResponse
{
    public ApplicationUser User { get; set; }

    public GetUserResponse()
    {
        User = new();
    }
}
