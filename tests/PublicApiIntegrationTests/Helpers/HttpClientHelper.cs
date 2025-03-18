using System.Net.Http;
using System.Net.Http.Headers;

namespace PublicApiIntegrationTests.Helpers;
internal static class HttpClientHelper
{
    public static HttpClient GetAdminClient()
    {
        return CreateClient(ApiTokenHelper.GetAdminUserToken());
    }
    public static HttpClient GetProductManagerClient()
    {
        return CreateClient(ApiTokenHelper.GetProductManagerUserToken());
    }
    public static HttpClient GetNormalUserClient()
    {
        return CreateClient(ApiTokenHelper.GetNormalUserToken());
    }

    private static HttpClient CreateClient(string token)
    {        
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
