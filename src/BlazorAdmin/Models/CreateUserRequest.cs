using System.ComponentModel.DataAnnotations;

namespace BlazorAdmin.Models;

public class CreateUserRequest
{
    [Required(ErrorMessage = "The Name field is required")]
    public string Name {  get; set; }
}
