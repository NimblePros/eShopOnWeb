using System.ComponentModel.DataAnnotations;

namespace BlazorAdmin.Models;

public class GetRoleMembershipRequest
{

    public GetRoleMembershipRequest(string name)
    {
        Name = name;
    }

    [Required(ErrorMessage = "The Name field is required")]
    public string Name { get; init; }
}
