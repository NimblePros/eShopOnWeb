using System.Text.RegularExpressions;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.Web.Pages.Basket;
using Xunit;
using static Microsoft.eShopWeb.FunctionalTests.Web.WebPageHelpers;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Basket;

[Collection(WebTestCollection.Name)]
public class IndexTest(TestApplication factory)
{
    // Each test gets its own client, so its own anonymous basket cookie.
    private readonly HttpClient _client = factory.CreateClient(allowAutoRedirect: true);

    [Fact]
    public async Task AddsItemToBasketGivenCatalogItemId()
    {
        var basketPage = await AddItemToBasketAsync(_client);

        Assert.Contains(MugProductNameHtml, basketPage);
        Assert.Equal("1", GetFirstBasketItemQuantity(basketPage));
    }

    [Fact]
    public async Task IncrementsQuantityGivenSameItemAddedTwice()
    {
        await AddItemToBasketAsync(_client);

        var basketPage = await AddItemToBasketAsync(_client);

        Assert.Single(Regex.Matches(basketPage, @"name=""Items\[\d+\]\.Id"""));
        Assert.Equal("2", GetFirstBasketItemQuantity(basketPage));
    }

    [Fact]
    public async Task RedirectsHomeWithoutAddingGivenUnknownCatalogItemId()
    {
        var ct = TestContext.Current.CancellationToken;
        var home = await _client.GetStringAsync("/", ct);

        var response = await PostFormAsync(_client, "/basket/index", home, KeyValuePair.Create("id", "999999"));

        response.EnsureSuccessStatusCode();
        Assert.Equal("/", response.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.Contains("Basket is empty", await _client.GetStringAsync("/basket", ct));
    }

    [Fact]
    public async Task OnPostUpdateTo50Successfully()
    {
        var basketPage = await AddItemToBasketAsync(_client);

        var updateResponse = await UpdateFirstBasketItemQuantityAsync(_client, basketPage, 49);

        var updatedPage = await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("/basket/update", updateResponse.RequestMessage!.RequestUri!.ToString());
        Assert.Contains((MugUnitPrice * 49).ToString("N2"), updatedPage);
    }

    [Fact]
    public async Task OnPostUpdateTo0EmptyBasket()
    {
        var basketPage = await AddItemToBasketAsync(_client);

        var updateResponse = await UpdateFirstBasketItemQuantityAsync(_client, basketPage, 0);

        Assert.Contains("/basket/update", updateResponse.RequestMessage!.RequestUri!.ToString());
        Assert.Contains("Basket is empty", await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task OnPostUpdateKeepsQuantityGivenNegativeQuantity()
    {
        var ct = TestContext.Current.CancellationToken;
        var basketPage = await AddItemToBasketAsync(_client);

        await UpdateFirstBasketItemQuantityAsync(_client, basketPage, -1);

        var reloaded = await _client.GetStringAsync("/basket", ct);
        Assert.Contains(MugProductNameHtml, reloaded);
        Assert.Equal("1", GetFirstBasketItemQuantity(reloaded));
    }

    [Fact]
    public async Task OnPostUpdateToMaxQuantitySucceeds()
    {
        var ct = TestContext.Current.CancellationToken;
        var basketPage = await AddItemToBasketAsync(_client);

        await UpdateFirstBasketItemQuantityAsync(_client, basketPage, BasketItem.MaxQuantity);

        Assert.Equal(BasketItem.MaxQuantity.ToString(), GetFirstBasketItemQuantity(await _client.GetStringAsync("/basket", ct)));
    }

    [Fact]
    public async Task OnPostUpdateKeepsQuantityAndShowsErrorGivenQuantityAboveMax()
    {
        var ct = TestContext.Current.CancellationToken;
        var basketPage = await AddItemToBasketAsync(_client);

        var updateResponse = await UpdateFirstBasketItemQuantityAsync(_client, basketPage, BasketItem.MaxQuantity + 1);

        var updatedPage = await updateResponse.Content.ReadAsStringAsync(ct);
        Assert.Contains($"Quantity must be between 0 and {BasketItem.MaxQuantity}.", updatedPage);
        // The basket is still rendered alongside the error, not replaced by an empty one.
        Assert.Contains(MugProductNameHtml, updatedPage);
        Assert.Equal("1", GetFirstBasketItemQuantity(await _client.GetStringAsync("/basket", ct)));
    }

    [Fact]
    public async Task AddingItemAlreadyAtMaxQuantityShowsMessageAndKeepsQuantity()
    {
        var basketPage = await AddItemToBasketAsync(_client);
        await UpdateFirstBasketItemQuantityAsync(_client, basketPage, BasketItem.MaxQuantity);

        var afterAdd = await AddItemToBasketAsync(_client);

        Assert.Contains(IndexModel.MaxQuantityReachedMessage, afterAdd);
        Assert.Equal(BasketItem.MaxQuantity.ToString(), GetFirstBasketItemQuantity(afterAdd));
    }

    [Fact]
    public async Task OnPostUpdateLeavesAnotherBasketUntouchedGivenItsItemId()
    {
        var ct = TestContext.Current.CancellationToken;
        var victimPage = await AddItemToBasketAsync(_client);
        var attacker = factory.CreateClient(allowAutoRedirect: true);
        var attackerPage = await AddItemToBasketAsync(attacker);

        // Post the victim's basket item id from the attacker's basket form.
        await PostFormAsync(attacker, "/basket/update", attackerPage,
            new("Items[0].Id", GetId(victimPage)),
            new("Items[0].Quantity", "0"));

        Assert.Equal("1", GetFirstBasketItemQuantity(await _client.GetStringAsync("/basket", ct)));
    }

    [Fact]
    public async Task LabelsEachQuantityInputWithItsProductName()
    {
        var basketPage = await AddItemToBasketAsync(_client);
        var itemId = GetId(basketPage);

        Assert.Contains($@"<label for=""basket-quantity-{itemId}"" class=""sr-only"">Quantity for {MugProductNameHtml}</label>", basketPage);
        var input = GetQuantityInput(basketPage, itemId);
        Assert.DoesNotContain("aria-invalid", input);
        Assert.DoesNotContain("aria-describedby", input);
        Assert.DoesNotContain("autofocus", input);
        Assert.Contains("<title>Basket - ", basketPage);
    }

    [Fact]
    public async Task OnPostUpdateMarksInputInvalidAndEchoesValueGivenQuantityAboveMax()
    {
        var basketPage = await AddItemToBasketAsync(_client);
        var itemId = GetId(basketPage);
        var tooMany = (BasketItem.MaxQuantity + 1).ToString();

        var updateResponse = await UpdateFirstBasketItemQuantityAsync(_client, basketPage, BasketItem.MaxQuantity + 1);

        var updatedPage = await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var input = GetQuantityInput(updatedPage, itemId);
        Assert.Contains(@"aria-invalid=""true""", input);
        Assert.Contains($@"aria-describedby=""basket-quantity-{itemId}-error""", input);
        Assert.Contains($@"value=""{tooMany}""", input);
        Assert.Matches($@"<span id=""basket-quantity-{itemId}-error"" class=""text-danger"">Quantity must be between 0 and {BasketItem.MaxQuantity}\.</span>", updatedPage);
        Assert.Contains(@"autofocus=""autofocus""", input);
        Assert.Contains($"<li>{IndexModel.NotSavedMessage}</li>", updatedPage);
        Assert.Contains($"<li>{MugProductNameHtml}: Quantity must be between 0 and {BasketItem.MaxQuantity}.</li>", updatedPage);
        Assert.Contains("<title>Error: Basket - ", updatedPage);
    }

    [Fact]
    public async Task OnPostUpdateMarksInputInvalidAndEchoesValueGivenFractionalQuantity()
    {
        var basketPage = await AddItemToBasketAsync(_client);
        var itemId = GetId(basketPage);

        // A number input accepts 2.5, but the quantity binds to an int.
        var updateResponse = await PostFormAsync(_client, "/basket/update", basketPage,
            new("Items[0].Id", itemId),
            new("Items[0].Quantity", "2.5"));

        var updatedPage = await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var input = GetQuantityInput(updatedPage, itemId);
        Assert.Contains(@"aria-invalid=""true""", input);
        Assert.Contains(@"value=""2.5""", input);
        Assert.Contains($@"<span id=""basket-quantity-{itemId}-error""", updatedPage);
        Assert.Contains($"<li>{MugProductNameHtml}: ", updatedPage);
    }

    [Fact]
    public async Task OnPostUpdateMarksOnlyTheInvalidLineAndEchoesEveryPostedValue()
    {
        await AddItemToBasketAsync(_client, SweatshirtCatalogItemId);
        var basketPage = await AddItemToBasketAsync(_client);
        var lineIds = GetLineIds(basketPage);
        Assert.Equal(2, lineIds.Count);

        var updateResponse = await PostFormAsync(_client, "/basket/update", basketPage,
            new("Items[0].Id", lineIds[0]),
            new("Items[0].Quantity", "7"),
            new("Items[1].Id", lineIds[1]),
            new("Items[1].Quantity", "-3"));

        var updatedPage = await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var validInput = GetQuantityInput(updatedPage, lineIds[0]);
        var invalidInput = GetQuantityInput(updatedPage, lineIds[1]);
        Assert.DoesNotContain("aria-invalid", validInput);
        Assert.Contains(@"value=""7""", validInput);
        Assert.Contains(@"aria-invalid=""true""", invalidInput);
        Assert.Contains(@"value=""-3""", invalidInput);
        Assert.Contains($@"aria-describedby=""basket-quantity-{lineIds[1]}-error""", invalidInput);
        Assert.Contains(@"autofocus=""autofocus""", invalidInput);
        Assert.Single(Regex.Matches(updatedPage, @"<span id=""basket-quantity-\d+-error"""));
        // The summary names the invalid line's product, and only that one.
        var summaryLines = Regex.Matches(updatedPage, "<li>(.*?)</li>").Select(m => m.Groups[1].Value).ToList();
        var invalidProduct = GetProductNameHtml(updatedPage, 1);
        var validProduct = GetProductNameHtml(updatedPage, 0);
        Assert.NotEqual(validProduct, invalidProduct);
        Assert.Contains($"{invalidProduct}: Quantity must be between 0 and {BasketItem.MaxQuantity}.", summaryLines);
        Assert.DoesNotContain(summaryLines, line => line.StartsWith(validProduct + ":"));
        // Nothing was saved: both lines keep their quantity of 1.
        var reloaded = await _client.GetStringAsync("/basket", TestContext.Current.CancellationToken);
        Assert.Equal(["1", "1"], Regex.Matches(reloaded, @"name=""Items\[\d+\]\.Quantity"" value=""(\d+)""").Select(m => m.Groups[1].Value));
    }

    [Fact]
    public async Task OnPostUpdateKeepsFirstValueAndErrorTogetherGivenRepeatedLineId()
    {
        var basketPage = await AddItemToBasketAsync(_client);
        var itemId = GetId(basketPage);

        var updateResponse = await PostFormAsync(_client, "/basket/update", basketPage,
            new("Items[0].Id", itemId),
            new("Items[0].Quantity", "-3"),
            new("Items[1].Id", itemId),
            new("Items[1].Quantity", "7"));

        var input = GetQuantityInput(await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), itemId);
        Assert.Contains(@"aria-invalid=""true""", input);
        Assert.Contains(@"value=""-3""", input);
    }

    [Fact]
    public async Task OnPostUpdateShowsSummaryErrorAndSavesNothingGivenRepeatedLineIdWithValidQuantities()
    {
        var ct = TestContext.Current.CancellationToken;
        var basketPage = await AddItemToBasketAsync(_client);
        var itemId = GetId(basketPage);

        var updateResponse = await PostFormAsync(_client, "/basket/update", basketPage,
            new("Items[0].Id", itemId),
            new("Items[0].Quantity", "2"),
            new("Items[1].Id", itemId),
            new("Items[1].Quantity", "3"));

        updateResponse.EnsureSuccessStatusCode();
        var updatedPage = await updateResponse.Content.ReadAsStringAsync(ct);
        Assert.Contains($"<li>{IndexModel.UpdateFailedMessage}</li>", updatedPage);
        Assert.Contains("<title>Error: Basket - ", updatedPage);
        Assert.Equal("1", GetFirstBasketItemQuantity(updatedPage));
        Assert.Equal("1", GetFirstBasketItemQuantity(await _client.GetStringAsync("/basket", ct)));
    }

    [Fact]
    public async Task OnPostUpdateShowsSummaryErrorGivenInvalidLineId()
    {
        var basketPage = await AddItemToBasketAsync(_client);

        var updateResponse = await PostFormAsync(_client, "/basket/update", basketPage,
            new("Items[0].Id", "not-an-id"),
            new("Items[0].Quantity", "3"));

        var updatedPage = await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains(IndexModel.UpdateFailedMessage, updatedPage);
        Assert.Equal("1", GetFirstBasketItemQuantity(updatedPage));
    }

    private const int SweatshirtCatalogItemId = 1;

    private static string GetQuantityInput(string basketPage, string basketItemId) =>
        Regex.Match(basketPage, $@"<input type=""number"" id=""basket-quantity-{basketItemId}""[^>]*>").Value;

    /// <summary>The product name in the label of the basket's <paramref name="line"/>-th quantity input.</summary>
    private static string GetProductNameHtml(string basketPage, int line) =>
        Regex.Matches(basketPage, @"class=""sr-only"">Quantity for (.*?)</label>")[line].Groups[1].Value;

    private static List<string> GetLineIds(string basketPage) =>
        Regex.Matches(basketPage, @"name=""Items\[\d+\]\.Id"" value=""(\d+)""").Select(m => m.Groups[1].Value).ToList();
}
