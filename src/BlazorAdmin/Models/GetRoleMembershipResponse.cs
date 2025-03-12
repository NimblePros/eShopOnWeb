using System.Collections.Generic;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace BlazorAdmin.Models;

public class GetRoleMembershipResponse
{
    public string RoleId { get; set; }
    public List<ApplicationUser> RoleMembers { get; set; } = new List<ApplicationUser>();
}
