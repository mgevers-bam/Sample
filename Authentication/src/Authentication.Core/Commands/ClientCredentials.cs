using Ardalis.Result;
using Authentication.Core.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace Authentication.Core.Commands;

public class ClientCredentialsCommand : IRequest<Result<TokenResponse>>
{
    public string ClientId { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public ICollection<string> Scopes { get; set; } = [];
}

public class ClientCredentialsCommandHandler(
    ILogger<ClientCredentialsCommandHandler> logger,
    IOpenIddictApplicationManager applicationManager,
    ITokenService tokenService)
    : IRequestHandler<ClientCredentialsCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(ClientCredentialsCommand request, CancellationToken cancellationToken)
    {
        var application = await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (application is null)
        {
            logger.LogWarning("Client credentials failed: Client {ClientId} not found.", request.ClientId);
            return Result.Unauthorized();
        }

        // Validate client secret if provided
        if (!string.IsNullOrEmpty(request.ClientSecret))
        {
            var isValid = await applicationManager.ValidateClientSecretAsync(application, request.ClientSecret, cancellationToken);
            if (!isValid)
            {
                logger.LogWarning("Client credentials failed: Invalid client secret for {ClientId}.", request.ClientId);
                return Result.Unauthorized();
            }
        }

        logger.LogInformation("Client {ClientId} authenticated successfully.", request.ClientId);

        var tokenResponse = await tokenService.GenerateClientTokensAsync(request.ClientId, request.Scopes, cancellationToken);
        return Result.Success(tokenResponse);
    }
}
