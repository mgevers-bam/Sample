using Ardalis.Result;
using Authentication.Core.Contracts;
using Authentication.Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace Authentication.Core.Commands;

public class RefreshTokenCommand : IRequest<Result<RefreshTokenResponse>>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
}

public class RefreshTokenCommandHandler(
    ILogger<RefreshTokenCommandHandler> logger,
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    JwtSecurityTokenHandler tokenHandler)
    : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await tokenService.GetValidRefreshToken(request.RefreshToken, cancellationToken);
        if (refreshToken == null)
        {
            logger.LogWarning("Invalid or expired refresh token attempted");
            return Result.Unauthorized("Invalid or expired refresh token");
        }

        var user = await userManager.FindByIdAsync(refreshToken.UserId);
        if (user == null)
        {
            logger.LogWarning("User {UserId} not found for refresh token", refreshToken.UserId);
            return Result.Unauthorized("User not found");
        }

        var token = await tokenService.GenerateAccessToken(user);
        logger.LogInformation("Access token refreshed for user {UserName}", user.UserName);

        return Result.Success(new RefreshTokenResponse
        {
            AccessToken = tokenHandler.WriteToken(token),
            TokenType = "Bearer",
            ExpiresIn = (int)Math.Ceiling(token.ValidTo.Subtract(DateTime.UtcNow).TotalSeconds)
        });
    }
}
