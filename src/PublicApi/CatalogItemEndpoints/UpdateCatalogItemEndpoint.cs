using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

/// <summary>
/// Updates a Catalog Item
/// </summary>
public class UpdateCatalogItemEndpoint(IRepository<CatalogItem> itemRepository, IUriComposer uriComposer)
    : Endpoint<UpdateCatalogItemRequest, Results<Ok<UpdateCatalogItemResponse>, NotFound>>
{
    public override void Configure()
    {
        Put("api/catalog-items");
        Roles(BlazorShared.Authorization.Constants.Roles.PRODUCT_MANAGERS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<UpdateCatalogItemResponse>()
             .WithTags("CatalogItemEndpoints"));
    }

    public override async Task<Results<Ok<UpdateCatalogItemResponse>, NotFound>> ExecuteAsync(UpdateCatalogItemRequest request, CancellationToken ct)
    {
        var response = new UpdateCatalogItemResponse(request.CorrelationId());

        var existingItem = await itemRepository.GetByIdAsync(request.Id, ct);
        if (existingItem == null)
        {
            return TypedResults.NotFound();
        }

        CatalogItem.CatalogItemDetails details = new(request.Name, request.Description, request.Price);
        existingItem.UpdateDetails(details);
        existingItem.UpdateBrand(request.CatalogBrandId);
        existingItem.UpdateType(request.CatalogTypeId);

        await itemRepository.UpdateAsync(existingItem, ct);

        var dto = new CatalogItemDto
        {
            Id = existingItem.Id,
            CatalogBrandId = existingItem.CatalogBrandId,
            CatalogTypeId = existingItem.CatalogTypeId,
            Description = existingItem.Description,
            Name = existingItem.Name,
            PictureUri = uriComposer.ComposePicUri(existingItem.PictureUri),
            Price = existingItem.Price
        };
        response.CatalogItem = dto;
        return TypedResults.Ok(response);
    }
}
