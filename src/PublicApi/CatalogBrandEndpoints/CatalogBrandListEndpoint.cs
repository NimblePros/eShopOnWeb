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
public class CatalogBrandListEndpoint : EndpointWithoutRequest<ListCatalogBrandsResponse>
{
    private readonly IRepository<CatalogBrand> _catalogBrandRepository;
    private readonly AutoMapper.IMapper _mapper;

    public CatalogBrandListEndpoint(IRepository<CatalogBrand> catalogBrandRepository, AutoMapper.IMapper mapper)
    {
        _catalogBrandRepository = catalogBrandRepository;
        _mapper = mapper;
    }

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

        var items = await _catalogBrandRepository.ListAsync(ct);

        response.CatalogBrands.AddRange(items.Select(_mapper.Map<CatalogBrandDto>));

        return response;
    }
}
