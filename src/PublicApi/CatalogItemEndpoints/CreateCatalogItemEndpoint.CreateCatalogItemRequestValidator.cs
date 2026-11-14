using FastEndpoints;
using FluentValidation;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

public class CreateCatalogItemRequestValidator : Validator<CreateCatalogItemRequest>
{
    // Matches HasMaxLength(50) on Name in CatalogItemConfiguration.
    public const int NameMaxLength = 50;

    public CreateCatalogItemRequestValidator()
    {
        RuleFor(x => x.CatalogBrandId).GreaterThan(0);
        RuleFor(x => x.CatalogTypeId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(NameMaxLength);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
