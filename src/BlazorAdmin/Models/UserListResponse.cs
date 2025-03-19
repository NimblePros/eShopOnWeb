using System.Collections.Generic;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace BlazorAdmin.Models;

public class UserListResponse
{
    public List<ApplicationUser> Users { get; set; } = [];
}
