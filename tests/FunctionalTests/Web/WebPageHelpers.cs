using System.Text.RegularExpressions;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web;

public static class WebPageHelpers
{
    public static string TokenTag = "__RequestVerificationToken";

    public static string GetRequestVerificationToken(string input)
    {
        string regexpression = @"name=""__RequestVerificationToken"" type=""hidden"" value=""([-A-Za-z0-9+=/\\_]+?)""";
        return RegexSearch(regexpression, input);
    }

    public const string DemoUserEmail = "demouser@microsoft.com";
    public const string DemoUserPassword = "Pass@word1";

    /// <summary>
    /// Signs the client in through the Identity login page. The client keeps the auth cookie
    /// for later requests.
    /// </summary>
    public static async Task<HttpResponseMessage> LoginAsync(HttpClient client,
        string email = DemoUserEmail,
        string password = DemoUserPassword)
    {
        var ct = TestContext.Current.CancellationToken;
        var getResponse = await client.GetAsync("/identity/account/login", ct);
        getResponse.EnsureSuccessStatusCode();
        var loginPage = await getResponse.Content.ReadAsStringAsync(ct);

        var formContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("Email", email),
            new("Password", password),
            new(TokenTag, GetRequestVerificationToken(loginPage))
        });

        return await client.PostAsync("/identity/account/login", formContent, ct);
    }

    public static string GetId(string input)
    {
        string regexpression = @"name=""Items\[0\].Id"" value=""(\d)""";
        return RegexSearch(regexpression, input);
    }

    private static string RegexSearch(string regexpression, string input)
    {
        var regex = new Regex(regexpression);
        var match = regex.Match(input);
        return match!.Groups!.Values!.LastOrDefault()!.Value;
    }
}
