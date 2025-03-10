using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

/// <summary>
/// Creates a new Catalog Item
/// </summary>
public class CreateCatalogItemEndpoint(IRepository<CatalogItem> itemRepository, IUriComposer uriComposer)
    : Endpoint<CreateCatalogItemRequest, CreateCatalogItemResponse>
{
    public override void Configure()
    {
        Post("api/catalog-items");
        Roles(BlazorShared.Authorization.Constants.Roles.PRODUCT_MANAGERS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<CreateCatalogItemResponse>()
             .WithTags("CatalogItemEndpoints"));
    }

    public override async Task HandleAsync(CreateCatalogItemRequest request, CancellationToken ct)
    {
        var response = new CreateCatalogItemResponse(request.CorrelationId());

        var catalogItemNameSpecification = new CatalogItemNameSpecification(request.Name);
        var existingCataloogItem = await itemRepository.CountAsync(catalogItemNameSpecification, ct);
        if (existingCataloogItem > 0)
        {
            throw new DuplicateException($"A catalogItem with name {request.Name} already exists");
        }

        var newItem = new CatalogItem(request.CatalogTypeId, request.CatalogBrandId, request.Description, request.Name, request.Price, request.PictureUri);
        newItem = await itemRepository.AddAsync(newItem, ct);

        if (newItem.Id != 0)
        {
            //We disabled the upload functionality and added a default/placeholder image to this sample due to a potential security risk 
            //  pointed out by the community. More info in this issue: https://github.com/dotnet-architecture/eShopOnWeb/issues/537 
            //  In production, we recommend uploading to a blob storage and deliver the image via CDN after a verification process.

            newItem.UpdatePictureUri("eCatalog-item-default.png");
            await itemRepository.UpdateAsync(newItem, ct);
        }

        var dto = new CatalogItemDto
        {
            Id = newItem.Id,
            CatalogBrandId = newItem.CatalogBrandId,
            CatalogTypeId = newItem.CatalogTypeId,
            Description = newItem.Description,
            Name = newItem.Name,
            PictureUri = uriComposer.ComposePicUri(newItem.PictureUri),
            Price = newItem.Price
        };
        response.CatalogItem = dto;

        await SendCreatedAtAsync<CatalogItemGetByIdEndpoint>(new{ CatalogItemId = dto.Id }, response, cancellation: ct);
    }
}
