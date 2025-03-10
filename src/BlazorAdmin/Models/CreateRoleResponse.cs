using Microsoft.AspNetCore.Identity;

namespace BlazorAdmin.Models;

public class CreateRoleResponse
{
    public IdentityRole Role { get; set; }
}
