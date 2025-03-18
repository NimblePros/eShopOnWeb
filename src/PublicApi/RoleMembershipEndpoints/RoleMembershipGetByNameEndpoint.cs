using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.PublicApi.RoleMembershipEndpoints;

public class RoleMembershipGetByNameEndpoint(UserManager<ApplicationUser> userManager) : Endpoint<GetRoleMembershipRequest, Results<Ok<GetRoleMembershipResponse>, NotFound>>
{
    public override void Configure()
    {
        Get("api/roles/{roleName}/members");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<GetRoleMembershipResponse>()
            .WithTags("RoleManagementEndpoints"));
    }

    public override async Task<Results<Ok<GetRoleMembershipResponse>, NotFound>> ExecuteAsync(GetRoleMembershipRequest request, CancellationToken ct)
    {
        var response = new GetRoleMembershipResponse(request.CorrelationId());

        var members = await userManager.GetUsersInRoleAsync(request.RoleName);

        if (members is null)
        {
            return TypedResults.NotFound();
        }

        response.RoleMembers = [.. members];
        return TypedResults.Ok(response);
    }
}
