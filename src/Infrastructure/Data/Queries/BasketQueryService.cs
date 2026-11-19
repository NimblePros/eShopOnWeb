using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.Infrastructure.Data.Queries;

public class BasketQueryService : IBasketQueryService
{
    private readonly CatalogContext _dbContext;

    public BasketQueryService(CatalogContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// This method performs the sum on the database rather than in memory.
    /// Lines whose catalog item no longer exists are not counted, matching the basket page.
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public async Task<int> CountTotalBasketItems(string username)
    {
        var totalItems = await _dbContext.Baskets
            .Where(basket => basket.BuyerId == username)
            .SelectMany(item => item.Items)
            .Where(basketItem => _dbContext.CatalogItems.Any(catalogItem => catalogItem.Id == basketItem.CatalogItemId))
            .SumAsync(sum => sum.Quantity);

        return totalItems;
    }
}
