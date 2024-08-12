using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.PublicApi.CatalogBrandEndpoints;

/// <summary>
/// List Catalog Brands
/// </summary>
public class CatalogBrandListEndpoint(IRepository<CatalogBrand> catalogBrandRepository, AutoMapper.IMapper mapper)
    : EndpointWithoutRequest<ListCatalogBrandsResponse>
{
    public override void Configure()
    {
        Get("api/catalog-brands");
        AllowAnonymous();
        Description(d =>
            d.Produces<ListCatalogBrandsResponse>()
           .WithTags("CatalogBrandEndpoints"));
    }

    public override async Task<ListCatalogBrandsResponse> ExecuteAsync(CancellationToken ct)
    {
        var response = new ListCatalogBrandsResponse();

        var items = await catalogBrandRepository.ListAsync(ct);

        response.CatalogBrands.AddRange(items.Select(mapper.Map<CatalogBrandDto>));

        return response;
    }
}
