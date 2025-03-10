using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class RoleListEndpoint(RoleManager<IdentityRole> roleManager):EndpointWithoutRequest<RoleListEndpointResponse>
{

    public override void Configure()
    {
        Get("api/roles");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d => d.Produces<RoleListEndpointResponse>()
        .WithTags("RoleManagementEndpoints"));
    }

    public override async Task<RoleListEndpointResponse> ExecuteAsync(CancellationToken ct)
    {
        await Task.Delay(1000, ct);
        var response = new RoleListEndpointResponse();
        response.Roles = roleManager.Roles.ToList();
        return response;
    }
}
