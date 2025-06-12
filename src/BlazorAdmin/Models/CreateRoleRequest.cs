using System.ComponentModel.DataAnnotations;

namespace BlazorAdmin.Models;

public class CreateRoleRequest
{
    [Required(ErrorMessage = "The Name field is required")]
    public string Name {  get; set; }
}
