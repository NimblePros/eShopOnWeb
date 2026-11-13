namespace BlazorAdmin.Authorization;

/// <summary>
/// Antiforgery request token from <c>GET /User/AntiforgeryToken</c>. Send it in the
/// <see cref="HeaderName"/> header on state-changing requests such as logout.
/// </summary>
public class AntiforgeryTokenResponse
{
    public const string HeaderName = "RequestVerificationToken";

    public string Token { get; set; }
}
