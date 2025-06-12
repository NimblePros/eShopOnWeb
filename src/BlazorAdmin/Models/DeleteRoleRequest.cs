using System.ComponentModel.DataAnnotations;

namespace BlazorAdmin.Models;

public class DeleteRoleRequest
{
    [Required(ErrorMessage = "The RoleId field is required")]
    public string RoleId { get; set; }
}
