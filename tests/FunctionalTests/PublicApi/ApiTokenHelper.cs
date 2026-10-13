using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Api;

public class ApiTokenHelper
{
    public static string GetAdminUserToken()
    {
        string userName = "admin@microsoft.com";
        string[] roles = { "Administrators" };

        return CreateToken(userName, roles);
    }

    public static string GetNormalUserToken()
    {
        string userName = "demouser@microsoft.com";
        string[] roles = { };

        return CreateToken(userName, roles);
    }

    private static string CreateToken(string userName, string[] roles)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, userName) };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Test hosts run in local development environments, which fall back to the development key
        var settings = new JwtSettings { SigningKey = JwtSettings.DEVELOPMENT_SIGNING_KEY };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims.ToArray()),
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(settings.GetSecurityKey(), SecurityAlgorithms.HmacSha256)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
