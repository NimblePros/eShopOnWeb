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
public class DeleteCatalogItemEndpoint(IRepository<CatalogItem> itemRepository) : Endpoint<DeleteCatalogItemRequest, Results<NoContent, NotFound>>
{
    public override void Configure()
    {
        Delete("api/catalog-items/{catalogItemId}");
        Roles(BlazorShared.Authorization.Constants.Roles.PRODUCT_MANAGERS);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d =>
        {
            d.Produces(StatusCodes.Status204NoContent);
            d.Produces(StatusCodes.Status404NotFound);
            d.WithTags("CatalogItemEndpoints");
        }
        );
    }

    public override async Task<Results<NoContent, NotFound>> ExecuteAsync(DeleteCatalogItemRequest request, CancellationToken ct)
    {

        var itemToDelete = await itemRepository.GetByIdAsync(request.CatalogItemId, ct);
        if (itemToDelete is null)
            return TypedResults.NotFound();

        await itemRepository.DeleteAsync(itemToDelete, ct);

        return TypedResults.NoContent();
    }
}
