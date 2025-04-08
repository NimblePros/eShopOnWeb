using System.Collections.Generic;

namespace BlazorAdmin.Models;

public class GetRoleMembershipResponse
{
    public string RoleId { get; set; }
    public List<UserForMembership> RoleMembers { get; set; } = new List<UserForMembership>();
}
