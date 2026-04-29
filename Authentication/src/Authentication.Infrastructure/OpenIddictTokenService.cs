using Authentication.Core.Contracts;
using Authentication.Core.Domain;
using Common.Infrastructure.Auth.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Authentication.Infrastructure;

/// <summary>
/// Token service that generates JWTs using OpenIddict's signing credentials.
/// </summary>
public class OpenIddictTokenService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOpenIddictScopeManager scopeManager,
    IOpenIddictApplicationManager applicationManager,
    IOptionsMonitor<OpenIddictServerOptions> openIddictServerOptions,
    JwtOptions options) : ITokenService
{
    public async Task<TokenResponse> GenerateTokensAsync(
        ApplicationUser user,
        ICollection<string> scopes,
        CancellationToken cancellationToken = default)
    {
        var claims = await BuildUserClaims(user, scopes, cancellationToken);
        return GenerateTokens(claims);
    }

    public async Task<TokenResponse> GenerateClientTokensAsync(
        string clientId,
        ICollection<string> scopes,
        CancellationToken cancellationToken = default)
    {
        var application = await applicationManager.FindByClientIdAsync(clientId, cancellationToken);
        var displayName = application is not null 
            ? await applicationManager.GetDisplayNameAsync(application, cancellationToken) 
            : clientId;

        var claims = await BuildClientClaims(clientId, displayName, scopes, cancellationToken);
        return GenerateTokens(claims, includeRefreshToken: false);
    }

    public async Task<TokenResponse?> RefreshTokensAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var principal = ValidateRefreshToken(refreshToken);
        if (principal is null)
            return null;

        var userId = principal.FindFirst(Claims.Subject)?.Value;
        if (string.IsNullOrEmpty(userId))
            return null;

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return null;

        // Get original scopes from the refresh token
        var scopes = principal.FindAll("scope").Select(c => c.Value).ToList();
        if (!scopes.Any())
            scopes = ["openid", "profile", "email"];

        var claims = await BuildUserClaims(user, scopes, cancellationToken);
        return GenerateTokens(claims);
    }

    private async Task<List<Claim>> BuildUserClaims(
        ApplicationUser user,
        ICollection<string> scopes,
        CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
        {
            new(Claims.Subject, user.Id),
            new(Claims.Name, user.UserName ?? string.Empty),
            new(Claims.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(Claims.Role, role)));

        // Add claims from each role
        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is not null)
            {
                var roleClaims = await roleManager.GetClaimsAsync(role);
                claims.AddRange(roleClaims);
            }
        }

        // Add scopes as claims
        claims.AddRange(scopes.Select(scope => new Claim("scope", scope)));

        // Add resources
        var scopesArray = scopes.ToImmutableArray();
        var resources = await scopeManager.ListResourcesAsync(scopesArray, cancellationToken).ToListAsync(cancellationToken);
        claims.AddRange(resources.Select(resource => new Claim("aud", resource)));

        var standaloneClaims = await userManager.GetClaimsAsync(user);
        claims.AddRange(standaloneClaims);

        return claims;
    }

    private async Task<List<Claim>> BuildClientClaims(
        string clientId,
        string? displayName,
        ICollection<string> scopes,
        CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
        {
            new(Claims.Subject, clientId),
            new(Claims.Name, displayName ?? clientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add scopes as claims
        claims.AddRange(scopes.Select(scope => new Claim("scope", scope)));

        // Add resources
        var scopesArray = scopes.ToImmutableArray();
        var resources = await scopeManager.ListResourcesAsync(scopesArray, cancellationToken).ToListAsync(cancellationToken);
        claims.AddRange(resources.Select(resource => new Claim("aud", resource)));

        return claims;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var serverOptions = openIddictServerOptions.CurrentValue;
        return serverOptions.SigningCredentials.FirstOrDefault()
            ?? throw new InvalidOperationException("No signing credentials configured in OpenIddict.");
    }

    private TokenResponse GenerateTokens(List<Claim> claims, bool includeRefreshToken = true)
    {
        var now = DateTime.UtcNow;
        var accessTokenExpiry = now.Add(options.AccessTokenLifetime);
        var signingCredentials = GetSigningCredentials();

        var tokenHandler = new JwtSecurityTokenHandler();

        // Create JWT with proper header including kid
        var header = new JwtHeader(signingCredentials);
        var payload = new JwtPayload(
            issuer: options.Issuer,
            audience: null,
            claims: claims,
            notBefore: now,
            expires: accessTokenExpiry,
            issuedAt: now);

        // Set token type
        header["typ"] = "at+jwt";

        var accessToken = new JwtSecurityToken(header, payload);

        var response = new TokenResponse
        {
            AccessToken = tokenHandler.WriteToken(accessToken),
            TokenType = "Bearer",
            ExpiresIn = (int)options.AccessTokenLifetime.TotalSeconds
        };

        if (includeRefreshToken)
        {
            response.RefreshToken = GenerateRefreshToken(claims, signingCredentials);
        }

        return response;
    }

    private string GenerateRefreshToken(List<Claim> claims, SigningCredentials signingCredentials)
    {
        var now = DateTime.UtcNow;
        var refreshTokenExpiry = now.Add(options.RefreshTokenLifetime);

        var tokenHandler = new JwtSecurityTokenHandler();

        var header = new JwtHeader(signingCredentials);
        header["typ"] = "rt+jwt";

        var payload = new JwtPayload(
            issuer: options.Issuer,
            audience: null,
            claims: claims,
            notBefore: now,
            expires: refreshTokenExpiry,
            issuedAt: now);

        var refreshToken = new JwtSecurityToken(header, payload);
        return tokenHandler.WriteToken(refreshToken);
    }

    private ClaimsPrincipal? ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var signingCredentials = GetSigningCredentials();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = signingCredentials.Key,
                ValidTypes = ["rt+jwt"]
            };

            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
