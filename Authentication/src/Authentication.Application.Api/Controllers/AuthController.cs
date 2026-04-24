using Ardalis.Result.AspNetCore;
using Authentication.Core.Commands;
using Authentication.Core.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Authentication.Application.Api.Controllers;

/// <summary>
/// Authentication endpoints for login, token refresh, and client credentials.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Authenticate with username and password.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginCommand
        {
            UserName = request.UserName,
            Password = request.Password,
            Scopes = request.Scopes ?? ["openid", "profile", "email", "stargate.api"]
        }, cancellationToken);

        IConvertToActionResult actionResult = result.ToActionResult(this);
        return actionResult.Convert();
    }

    /// <summary>
    /// Refresh an access token using a refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken
        }, cancellationToken);

        IConvertToActionResult actionResult = result.ToActionResult(this);
        return actionResult.Convert();
    }

    /// <summary>
    /// Authenticate using client credentials (service-to-service).
    /// </summary>
    [HttpPost("client-token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ClientToken([FromBody] ClientCredentialsRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ClientCredentialsCommand
        {
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,
            Scopes = request.Scopes ?? []
        }, cancellationToken);

        IConvertToActionResult actionResult = result.ToActionResult(this);
        return actionResult.Convert();
    }
}

/// <summary>
/// Request for username/password authentication.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The username.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// The password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Optional scopes to request. Defaults to openid, profile, email.
    /// </summary>
    public IEnumerable<string>? Scopes { get; set; }
}

/// <summary>
/// Request for refreshing an access token.
/// </summary>
public class RefreshRequest
{
    /// <summary>
    /// The refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Request for client credentials authentication.
/// </summary>
public class ClientCredentialsRequest
{
    /// <summary>
    /// The client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The client secret.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Scopes to request.
    /// </summary>
    public IEnumerable<string>? Scopes { get; set; }
}
