using Authentication.Core.Domain;

namespace Authentication.Core.Contracts;

/// <summary>
/// Service for generating authentication tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates access and refresh tokens for a user.
    /// </summary>
    Task<TokenResponse> GenerateTokensAsync(
        ApplicationUser user,
        ICollection<string> scopes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates tokens for a client application.
    /// </summary>
    Task<TokenResponse> GenerateClientTokensAsync(
        string clientId,
        ICollection<string> scopes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes tokens using a refresh token.
    /// </summary>
    Task<TokenResponse?> RefreshTokensAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}
