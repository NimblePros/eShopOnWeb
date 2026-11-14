using FastEndpoints;
using FluentValidation;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

public class CreateCatalogItemRequestValidator : Validator<CreateCatalogItemRequest>
{
    // Matches HasMaxLength(50) on Name in CatalogItemConfiguration.
    public const int NameMaxLength = 50;

    // Price is stored as decimal(18,2), which holds at most 16 digits before the decimal point.
    public const decimal PriceUpperBound = 10_000_000_000_000_000m;

    public CreateCatalogItemRequestValidator()
    {
        RuleFor(x => x.CatalogBrandId).GreaterThan(0);
        RuleFor(x => x.CatalogTypeId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(NameMaxLength);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0).LessThan(PriceUpperBound);
    }
}
