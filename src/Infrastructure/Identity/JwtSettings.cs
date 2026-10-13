using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.eShopWeb.Infrastructure.Identity;

public class JwtSettings
{
    public const string CONFIG_NAME = "Authentication:Jwt";

    // Publicly known key so the sample runs locally with no setup.
    // It is only accepted in local development environments; see JwtSettingsValidator.
    public const string DEVELOPMENT_SIGNING_KEY = "SecretKeyOfDoomThatMustBeAMinimumNumberOfBytes";

    // HS256 requires a key of at least 256 bits.
    public const int MINIMUM_KEY_BYTES = 32;

    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "eShopOnWeb";
    public string Audience { get; set; } = "eShopOnWeb";
    public int TokenLifetimeMinutes { get; set; } = 60;

    public SymmetricSecurityKey GetSecurityKey() => new(Encoding.UTF8.GetBytes(SigningKey));
}
