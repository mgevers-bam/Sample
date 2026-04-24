namespace Authentication.Core.Contracts;

/// <summary>
/// Token response containing access and refresh tokens.
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// The access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// The refresh token (if issued).
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// The token type (typically "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Token expiration time in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }
}
