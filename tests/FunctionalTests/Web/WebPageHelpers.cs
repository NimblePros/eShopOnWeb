using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;
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

    /// <summary>
    /// Posts form fields plus the antiforgery token scraped from <paramref name="pageWithForm"/>.
    /// </summary>
    public static Task<HttpResponseMessage> PostFormAsync(HttpClient client, string url, string pageWithForm,
        params KeyValuePair<string, string>[] fields)
    {
        var values = new List<KeyValuePair<string, string>>(fields)
        {
            new(TokenTag, GetRequestVerificationToken(pageWithForm))
        };
        return client.PostAsync(url, new FormUrlEncodedContent(values), TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Creates a throwaway user with a unique email, so tests that change account state don't
    /// affect the demo user other tests sign in as. Returns the email.
    /// </summary>
    public static async Task<string> CreateUserAsync(TestApplication factory, string password = DemoUserPassword)
    {
        var email = $"user-{Guid.NewGuid():N}@example.com";
        await UseUserManagerAsync(factory, async userManager =>
        {
            var result = await userManager.CreateAsync(new ApplicationUser { UserName = email, Email = email }, password);
            Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));
            return result;
        });
        return email;
    }

    public static async Task<T> UseUserManagerAsync<T>(TestApplication factory,
        Func<UserManager<ApplicationUser>, Task<T>> action)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return await action(userManager);
    }

    public static Task<ApplicationUser> FindUserAsync(TestApplication factory, string email) =>
        UseUserManagerAsync(factory, async userManager => (await userManager.FindByEmailAsync(email))!);

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
