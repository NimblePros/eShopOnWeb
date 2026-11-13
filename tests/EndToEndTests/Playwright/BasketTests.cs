using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace Microsoft.eShopWeb.EndToEndTests.Playwright;

public sealed class BasketTests(BrowserFixture browserFixture) : IClassFixture<BrowserFixture>
{
    [Fact(Skip = "Intentional failure demo. Remove Skip to observe screenshot/video/trace artifacts.")]
    public async Task Cart_AddItem_ShowsExpectedTotal()
    {
        (IBrowserContext context, string outputDirectory) =
            await browserFixture.CreateDebugContextAsync(nameof(Cart_AddItem_ShowsExpectedTotal));

        IPage page = await context.NewPageAsync();

        try
        {
            await page.GotoAsync(browserFixture.BaseUrl);
            await page.GetByRole(AriaRole.Button, new() { Name = "Add to Basket" }).First.ClickAsync();
            await page.GetByRole(AriaRole.Link, new() { Name = "Basket" }).ClickAsync();

            // Intentionally incorrect assertion for debugging workflow demos.
            await Expect(page.GetByText("$999.99")).ToBeVisibleAsync();
        }
        catch
        {
            await browserFixture.CaptureFailureArtifactsAsync(context, page, outputDirectory);
            throw;
        }
        finally
        {
            await context.CloseAsync();
        }
    }
}
