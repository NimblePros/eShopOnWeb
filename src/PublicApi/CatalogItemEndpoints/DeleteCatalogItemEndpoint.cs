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
/// Deletes a Catalog Item
/// </summary>
public class DeleteCatalogItemEndpoint : Endpoint<DeleteCatalogItemRequest, Results<Ok<DeleteCatalogItemResponse>, NotFound>>
{
    private readonly IRepository<CatalogItem> _itemRepository;

    public DeleteCatalogItemEndpoint(IRepository<CatalogItem> itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public override void Configure()
    {
        Delete("api/catalog-items/{catalogItemId}");
        Roles(BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
            d.Produces<DeleteCatalogItemResponse>()
             .WithTags("CatalogItemEndpoints"));
    }

    public override async Task<Results<Ok<DeleteCatalogItemResponse>, NotFound>> ExecuteAsync(DeleteCatalogItemRequest request, CancellationToken ct)
    {
        var response = new DeleteCatalogItemResponse(request.CorrelationId());

        var itemToDelete = await _itemRepository.GetByIdAsync(request.CatalogItemId, ct);
        if (itemToDelete is null)
            return TypedResults.NotFound();

        await _itemRepository.DeleteAsync(itemToDelete, ct);

        return TypedResults.Ok(response);
    }
}
