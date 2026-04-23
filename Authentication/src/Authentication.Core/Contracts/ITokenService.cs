using Authentication.Core.Domain;
using System.IdentityModel.Tokens.Jwt;

namespace Authentication.Core.Contracts;

public interface ITokenService
{
    Task<JwtSecurityToken> GenerateAccessToken(ApplicationUser user);
    Task<string> GenerateRefreshToken(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<JwtSecurityToken?> ValidateAccessToken(string token, CancellationToken cancellationToken = default);
    Task RevokeRefreshToken(string token, string? reason = null, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetValidRefreshToken(string token, CancellationToken cancellationToken = default);
}

