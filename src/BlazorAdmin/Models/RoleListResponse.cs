using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace BlazorAdmin.Models;

public class RoleListResponse
{
    public List<IdentityRole> Roles { get; set; } = new List<IdentityRole>();
}
