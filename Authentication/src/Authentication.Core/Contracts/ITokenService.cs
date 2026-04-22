using Authentication.Core.Domain;
using System.IdentityModel.Tokens.Jwt;

namespace Authentication.Core.Contracts;

public interface ITokenService
{
    Task<JwtSecurityToken> GenerateAccessToken(ApplicationUser user);
    Task<string> GenerateRefreshToken(ApplicationUser user);
    Task<JwtSecurityToken?> ValidateAccessToken(string token);
    Task RevokeRefreshToken(string token, string? reason = null);
    Task<RefreshToken?> GetValidRefreshToken(string token);
}

