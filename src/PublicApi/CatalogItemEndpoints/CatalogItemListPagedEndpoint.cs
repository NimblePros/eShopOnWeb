using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

/// <summary>
/// List Catalog Items (paged)
/// </summary>
public class CatalogItemListPagedEndpoint(IRepository<CatalogItem> itemRepository, IUriComposer uriComposer,
        AutoMapper.IMapper mapper, ILogger<CatalogItemListPagedEndpoint> logger)
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
        await Task.Delay(1000, ct);

        var response = new ListPagedCatalogItemResponse(request.CorrelationId());

        var filterSpec = new CatalogFilterSpecification(request.CatalogBrandId, request.CatalogTypeId);
        int totalItems = await itemRepository.CountAsync(filterSpec, ct);

        var pagedSpec = new CatalogFilterPaginatedSpecification(
            skip: request.PageIndex * request.PageSize,
            take: request.PageSize,
            brandId: request.CatalogBrandId,
            typeId: request.CatalogTypeId);

        var items = await itemRepository.ListAsync(pagedSpec, ct);

        logger.LogInformation("Retrieved {ItemCount} catalog items from database for page {PageIndex} with page size {PageSize}. Total items available: {TotalItems}", 
            items.Count, request.PageIndex, request.PageSize, totalItems);

        response.CatalogItems.AddRange(items.Select(mapper.Map<CatalogItemDto>));
        foreach (CatalogItemDto item in response.CatalogItems)
        {
            item.PictureUri = uriComposer.ComposePicUri(item.PictureUri);
        }

        if (request.PageSize > 0)
        {
            response.PageCount = (int) Math.Ceiling((decimal)totalItems / request.PageSize);
        }
        else
        {
            response.PageCount = totalItems > 0 ? 1 : 0;
        }

        return response;
    }
}
