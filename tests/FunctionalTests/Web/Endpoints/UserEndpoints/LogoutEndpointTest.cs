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
    public async Task SignsOutSignedInUserGivenAntiforgeryToken()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await WebPageHelpers.LoginAsync(client);
        var before = await client.GetFromJsonAsync<UserInfo>("/User", ct);
        Assert.True(before!.IsAuthenticated);

        var response = await PostLogoutAsync(client, await GetAntiforgeryTokenAsync(client));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var after = await client.GetFromJsonAsync<UserInfo>("/User", ct);
        Assert.False(after!.IsAuthenticated);
    }

    [Fact]
    public async Task RejectsLogoutWithoutAntiforgeryToken()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await WebPageHelpers.LoginAsync(client);

        var response = await client.PostAsync("/User/Logout", content: null, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var after = await client.GetFromJsonAsync<UserInfo>("/User", ct);
        Assert.True(after!.IsAuthenticated);
    }

    [Fact]
    public async Task RejectsLogoutWithInvalidAntiforgeryToken()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await WebPageHelpers.LoginAsync(client);
        // Get the antiforgery cookie, then send a token that doesn't match it.
        await GetAntiforgeryTokenAsync(client);

        var response = await PostLogoutAsync(client, "not-a-real-token");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var after = await client.GetFromJsonAsync<UserInfo>("/User", ct);
        Assert.True(after!.IsAuthenticated);
    }

    [Fact]
    public async Task RejectsTokenIssuedBeforeSignIn()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var anonymousToken = await GetAntiforgeryTokenAsync(client);
        await WebPageHelpers.LoginAsync(client);

        var response = await PostLogoutAsync(client, anonymousToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

        var response = await PostLogoutAsync(client, await GetAntiforgeryTokenAsync(client));
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
    public async Task ReturnsOkGivenAnonymousUserWithAntiforgeryToken()
    {
        var client = _factory.CreateClient();

        var response = await PostLogoutAsync(client, await GetAntiforgeryTokenAsync(client));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RejectsAnonymousLogoutWithoutAntiforgeryToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/User/Logout", content: null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<AntiforgeryTokenResponse>("/User/AntiforgeryToken",
            TestContext.Current.CancellationToken);
        return response!.Token;
    }

    private static Task<HttpResponseMessage> PostLogoutAsync(HttpClient client, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/User/Logout");
        request.Headers.Add(AntiforgeryTokenResponse.HeaderName, token);
        return client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
