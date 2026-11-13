using System.Net.Http.Json;
using BlazorAdmin.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Endpoints.UserEndpoints;

[Collection("Sequential")]
public class GetCurrentUserEndpointTest(TestApplication factory) : IClassFixture<TestApplication>
{
    private readonly TestApplication _factory = factory;

    [Fact]
    public async Task ReturnsAnonymousUserInfoGivenNoSignedInUser()
    {
        var client = _factory.CreateClient();

        var userInfo = await client.GetFromJsonAsync<UserInfo>("/User", TestContext.Current.CancellationToken);

        Assert.NotNull(userInfo);
        Assert.False(userInfo.IsAuthenticated);
        Assert.Null(userInfo.Token);
    }

    [Fact]
    public async Task ReturnsClaimsAndTokenGivenSignedInUser()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await WebPageHelpers.LoginAsync(client);

        var userInfo = await client.GetFromJsonAsync<UserInfo>("/User", TestContext.Current.CancellationToken);

        Assert.NotNull(userInfo);
        Assert.True(userInfo.IsAuthenticated);
        Assert.False(string.IsNullOrEmpty(userInfo.Token));
        Assert.False(string.IsNullOrEmpty(userInfo.NameClaimType));
        Assert.Contains(userInfo.Claims, c => c.Type == userInfo.NameClaimType && c.Value == WebPageHelpers.DemoUserEmail);
    }
}
