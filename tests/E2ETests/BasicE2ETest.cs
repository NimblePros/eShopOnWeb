using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace Microsoft.eShopWeb.E2ETests;

public class BasicE2ETest
{
    [Fact]
    public async Task HomePageLoads()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync("http://localhost:5106");
        var title = await page.TitleAsync();
        Assert.Contains("eShop", title);
    }
}
