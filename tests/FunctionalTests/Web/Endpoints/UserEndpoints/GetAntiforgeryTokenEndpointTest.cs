using System.Net.Http.Json;
using BlazorAdmin.Authorization;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Endpoints.UserEndpoints;

[Collection("Sequential")]
public class GetAntiforgeryTokenEndpointTest(TestApplication factory) : IClassFixture<TestApplication>
{
    private readonly TestApplication _factory = factory;

    [Fact]
    public async Task ReturnsTokenAndSetsAntiforgeryCookie()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/User/AntiforgeryToken", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AntiforgeryTokenResponse>(TestContext.Current.CancellationToken);
        Assert.False(string.IsNullOrEmpty(body!.Token));
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var setCookies));
        Assert.Contains(setCookies, c => c.StartsWith(".AspNetCore.Antiforgery.", StringComparison.Ordinal));
    }
}
