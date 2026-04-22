using Ardalis.Result;
using Authentication.Core.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authentication.Core.Commands;

public class RevokeAccessTokenCommand : IRequest<Result>
{
    public string AccessToken { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class RevokeAccessTokenCommandHandler(
    ILogger<RevokeAccessTokenCommandHandler> logger,
    ITokenService tokenService)
    : IRequestHandler<RevokeAccessTokenCommand, Result>
{
    public async Task<Result> Handle(RevokeAccessTokenCommand request, CancellationToken cancellationToken)
    {
        // Validate token first
        var validToken = await tokenService.ValidateAccessToken(request.AccessToken);
        if (validToken == null)
        {
            logger.LogWarning("Attempted to revoke invalid or expired token");
            return Result.Unauthorized("Invalid or expired token");
        }

        // Extract JTI (JWT ID) from token
        var jti = validToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
        if (string.IsNullOrEmpty(jti))
        {
            return Result.Error("Token does not contain required identifier");
        }

        // TODO: Implement token revocation logic
        // This would involve adding the JTI to a revoked tokens list in the database

        logger.LogInformation("Access token revoked for JTI {JTI}, reason: {Reason}", jti, request.Reason ?? "Not specified");
        return Result.Success();
    }
}
