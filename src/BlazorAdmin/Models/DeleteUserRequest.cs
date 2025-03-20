using System.ComponentModel.DataAnnotations;

namespace BlazorAdmin.Models;

public class DeleteUserRequest
{
    [Required(ErrorMessage = "The UserId field is required")]
    public string UserId { get; set; }
}
