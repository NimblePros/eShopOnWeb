using FastEndpoints;
using FluentValidation;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

public class ListPagedCatalogItemRequestValidator : Validator<ListPagedCatalogItemRequest>
{
    public ListPagedCatalogItemRequestValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, ListPagedCatalogItemRequest.MaxPageSize);
        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(0)
            // Keeps the Skip offset (PageIndex * PageSize) from overflowing int.
            .Must((request, pageIndex) => (long)pageIndex * request.PageSize <= int.MaxValue)
            .WithMessage("'Page Index' is too large for the requested page size.");
    }
}
