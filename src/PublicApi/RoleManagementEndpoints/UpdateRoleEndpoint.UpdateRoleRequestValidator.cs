using FastEndpoints;
using FluentValidation;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class UpdateRoleRequestValidator : Validator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}
