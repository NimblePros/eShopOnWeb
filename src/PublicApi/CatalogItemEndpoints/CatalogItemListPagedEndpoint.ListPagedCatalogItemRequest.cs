namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

public class ListPagedCatalogItemRequest : BaseRequest
{
    // Upper bound on items per page; also the page size used when the caller omits one.
    public const int MaxPageSize = 100;

    public int PageSize { get; init; }
    public int PageIndex { get; init; }
    public int? CatalogBrandId { get; init; }
    public int? CatalogTypeId { get; init; }

    public ListPagedCatalogItemRequest(int? pageSize, int? pageIndex, int? catalogBrandId, int? catalogTypeId)
    {
        PageSize = pageSize ?? MaxPageSize;
        PageIndex = pageIndex ?? 0;
        CatalogBrandId = catalogBrandId;
        CatalogTypeId = catalogTypeId;
    }
}
