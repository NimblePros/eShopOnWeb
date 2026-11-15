using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

/// <summary>
/// List Catalog Items (paged)
/// </summary>
public class CatalogItemListPagedEndpoint(IRepository<CatalogItem> itemRepository, IUriComposer uriComposer)
    : Endpoint<ListPagedCatalogItemRequest, ListPagedCatalogItemResponse>
{
    public override void Configure()
    {
        Get("api/catalog-items");
        AllowAnonymous();
        Description(d =>
            d.Produces<ListPagedCatalogItemResponse>()
             .WithTags("CatalogItemEndpoints"));
    }

    public override async Task<ListPagedCatalogItemResponse> ExecuteAsync(ListPagedCatalogItemRequest request, CancellationToken ct)
    {
        var response = new ListPagedCatalogItemResponse(request.CorrelationId());

        var filterSpec = new CatalogFilterSpecification(request.CatalogBrandId, request.CatalogTypeId);
        int totalItems = await itemRepository.CountAsync(filterSpec, ct);

        var pagedSpec = new CatalogFilterPaginatedSpecification(
            skip: request.PageIndex * request.PageSize,
            take: request.PageSize,
            brandId: request.CatalogBrandId,
            typeId: request.CatalogTypeId);

        var items = await itemRepository.ListAsync(pagedSpec, ct);

        response.CatalogItems.AddRange(items.Select(item => item.ToDto()));
        foreach (CatalogItemDto item in response.CatalogItems)
        {
            item.PictureUri = uriComposer.ComposePicUri(item.PictureUri);
        }

        // The validator guarantees PageSize is between 1 and MaxPageSize.
        response.PageCount = (int)Math.Ceiling((decimal)totalItems / request.PageSize);

        return response;
    }
}
