using System.Net;
using System.Net.Http.Json;
using BlazorAdmin.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Endpoints.UserEndpoints;

[Collection("Sequential")]
public class LogoutEndpointTest(TestApplication factory) : IClassFixture<TestApplication>
{
    private readonly TestApplication _factory = factory;

    [Fact]
    public async Task SignsOutSignedInUser()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await WebPageHelpers.LoginAsync(client);
        var before = await client.GetFromJsonAsync<UserInfo>("/User", ct);
        Assert.True(before!.IsAuthenticated);

        var response = await client.PostAsync("/User/Logout", content: null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var after = await client.GetFromJsonAsync<UserInfo>("/User", ct);
        Assert.False(after!.IsAuthenticated);
    }

    [Fact]
    public async Task RevokesAuthCookieSoReplayingItIsAnonymous()
    {
        var ct = TestContext.Current.CancellationToken;
        var cookies = new CookieContainer();
        var client = _factory.CreateDefaultClient(new CookieContainerHandler(cookies));
        await WebPageHelpers.LoginAsync(client);
        var authCookieHeader = cookies.GetCookieHeader(client.BaseAddress!);
        Assert.Contains("EshopIdentifier=", authCookieHeader);

        var response = await client.PostAsync("/User/Logout", content: null, ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Replay the cookie captured before logout from a client that ignores the logout's Set-Cookie.
        var replayClient = _factory.CreateDefaultClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/User");
        request.Headers.Add("Cookie", authCookieHeader);
        var replayResponse = await replayClient.SendAsync(request, ct);

        replayResponse.EnsureSuccessStatusCode();
        var userInfo = await replayResponse.Content.ReadFromJsonAsync<UserInfo>(ct);
        Assert.False(userInfo!.IsAuthenticated);
    }

    [Fact]
    public async Task ReturnsOkGivenAnonymousUser()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/User/Logout", content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
