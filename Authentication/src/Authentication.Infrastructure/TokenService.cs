using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Authentication.Core.Contracts;
using Authentication.Core.Domain;
using Authentication.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Common.Infrastructure.Auth.Options;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure;

public class TokenService(
    UserManager<ApplicationUser> userManager,
    AuthenticationDbContext dbContext,
    JwtOptions options) : ITokenService
{
    public async Task<JwtSecurityToken> GenerateAccessToken(ApplicationUser user)
    {
        var signingKey = options.SigningKey
            ?? throw new InvalidOperationException($"{nameof(JwtOptions.SigningKey)} is missing");

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var roles = await userManager.GetRolesAsync(user);
        var roleClaims = roles
            .Select(role => new Claim(ClaimTypes.Role, role));

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("jti", Guid.NewGuid().ToString()), // JWT ID for token revocation tracking
        ];

        return new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims.Concat(roleClaims),
            expires: DateTime.UtcNow.Add(options.AccessTokenLifetime),
            signingCredentials: credentials);
    }

    public async Task<string> GenerateRefreshToken(ApplicationUser user)
    {
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)),
            ExpiresAt = DateTime.UtcNow.Add(options.RefreshTokenLifetime),
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync();

        return refreshToken.Token;
    }

    public async Task<JwtSecurityToken?> ValidateAccessToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var signingKey = options.SigningKey
            ?? throw new InvalidOperationException($"{nameof(JwtOptions.SigningKey)} is missing");

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,
                ValidateAudience = true,
                ValidAudience = options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
                return null;

            // Check if token is revoked
            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
            if (!string.IsNullOrEmpty(jti))
            {
                var isRevoked = await dbContext.RevokedTokens.AnyAsync(rt => rt.JwtTokenId == jti);
                if (isRevoked)
                    return null;
            }

            return jwtToken;
        }
        catch
        {
            return null;
        }
    }

    public async Task RevokeRefreshToken(string token, string? reason = null)
    {
        var refreshToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken != null)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<RefreshToken?> GetValidRefreshToken(string token)
    {
        return await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.IsValid);
    }
}
