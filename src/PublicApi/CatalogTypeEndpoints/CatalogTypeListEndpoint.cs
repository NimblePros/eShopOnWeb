using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.PublicApi.CatalogTypeEndpoints;

/// <summary>
/// List Catalog Types
/// </summary>
public class CatalogTypeListEndpoint(IRepository<CatalogType> catalogTypeRepository, AutoMapper.IMapper mapper)
    : EndpointWithoutRequest<ListCatalogTypesResponse>
{
    public override void Configure()
    {
        Get("api/catalog-types");
        AllowAnonymous();
        Description(d =>
            d.Produces<ListCatalogTypesResponse>()
             .WithTags("CatalogTypeEndpoints"));
    }

    public override async Task<ListCatalogTypesResponse> ExecuteAsync(CancellationToken ct)
    {
        var response = new ListCatalogTypesResponse();

        var items = await catalogTypeRepository.ListAsync(ct);

        response.CatalogTypes.AddRange(items.Select(mapper.Map<CatalogTypeDto>));

        return response;
    }
}
