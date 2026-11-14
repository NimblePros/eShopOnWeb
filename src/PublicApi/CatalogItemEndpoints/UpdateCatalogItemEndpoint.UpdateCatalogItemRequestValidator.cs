using FastEndpoints;
using FluentValidation;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

public class UpdateCatalogItemRequestValidator : Validator<UpdateCatalogItemRequest>
{
    public UpdateCatalogItemRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.CatalogBrandId).GreaterThan(0);
        RuleFor(x => x.CatalogTypeId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(CreateCatalogItemRequestValidator.NameMaxLength);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0).LessThan(CreateCatalogItemRequestValidator.PriceUpperBound);
    }
}
