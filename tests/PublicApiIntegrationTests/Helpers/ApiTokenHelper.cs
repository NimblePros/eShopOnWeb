using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PublicApiIntegrationTests.Helpers
{
    public class ApiTokenHelper
    {
        public static string GetAdminUserToken()
        {
            var userName = "admin@microsoft.com";
            string[] roles = { "Administrators" };

            return CreateToken(userName, roles);
        }

        public static string GetProductManagerUserToken()
        {
            var userName = "productmgr@microsoft.com";
            string[] roles = { "Product Managers" };

            return CreateToken(userName, roles);
        }

        public static string GetNormalUserToken()
        {
            var userName = "demouser@microsoft.com";
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
}
