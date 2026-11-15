using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.ApplicationCore.Constants;
using Microsoft.eShopWeb.PublicApi.AuthEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PublicApiIntegrationTests.AuthEndpoints;

[TestClass]
public class AuthenticateEndpoint
{
    [TestMethod]
    [DataRow("demouser@eshop.test", AuthorizationConstants.DEFAULT_PASSWORD, true)]
    [DataRow("demouser@eshop.test", "badpassword", false)]
    [DataRow("baduser@eshop.test", "badpassword", false)]
    public async Task ReturnsExpectedResultGivenCredentials(string testUsername, string testPassword, bool expectedResult)
    {
        var request = new AuthenticateRequest()
        {
            Username = testUsername,
            Password = testPassword
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await ProgramTest.NewClient.PostAsync("api/authenticate", jsonContent);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync();
        var model = stringResponse.FromJson<AuthenticateResponse>();

        Assert.AreEqual(expectedResult, model!.Result);
    }
}
