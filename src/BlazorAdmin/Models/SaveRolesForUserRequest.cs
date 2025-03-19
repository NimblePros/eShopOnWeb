using System.Collections.Generic;

namespace BlazorAdmin.Models;

public class SaveRolesForUserRequest
{
    public string UserId { get; set; }
    public List<string> RolesToAdd { get; set; } = [];
    public List<string> RolesToRemove { get; set; } = [];

}
