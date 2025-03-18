using System.Net.Http;
using System.Net.Http.Headers;

namespace PublicApiIntegrationTests;
public class PublicApiTestBase
{
    internal HttpClient GetAdminClient()
    {
        var adminToken = ApiTokenHelper.GetAdminUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        return client;
    }
    internal HttpClient GetProductManagerClient()
    {
        var adminToken = ApiTokenHelper.GetProductManagerUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        return client;
    }

    internal HttpClient GetNormalUserClient()
    {
        var adminToken = ApiTokenHelper.GetNormalUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        return client;
    }
}
