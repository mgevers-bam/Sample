using Ardalis.Result;
using Authentication.Core.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authentication.Core.Commands;

public class RefreshTokenCommand : IRequest<Result<TokenResponse>>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenCommandHandler(
    ILogger<RefreshTokenCommandHandler> logger,
    ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenResponse = await tokenService.RefreshTokensAsync(request.RefreshToken, cancellationToken);

        if (tokenResponse is null)
        {
            logger.LogWarning("Refresh token failed: Invalid or expired refresh token.");
            return Result.Unauthorized();
        }

        logger.LogInformation("Token refreshed successfully.");
        return Result.Success(tokenResponse);
    }
}
