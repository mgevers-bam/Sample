using Ardalis.Result.AspNetCore;
using Authentication.Core.Commands;
using Authentication.Core.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Security.Claims;

namespace Authentication.Application.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IMediator mediator) : ControllerBase
{

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        IConvertToActionResult actionResult = result.ToActionResult(this);
        return actionResult.Convert();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet("user-info")]
    [Authorize]
    public async Task<IActionResult> GetUserInfo(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var query = new GetUserInfoQuery { UserId = userId };
        var result = await mediator.Send(query, cancellationToken);

        IConvertToActionResult actionResult = result.ToActionResult(this);
        return actionResult.Convert();
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        IConvertToActionResult actionResult = result.ToActionResult(this);
        return actionResult.Convert();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        command.UserId = userId;
        var result = await mediator.Send(command, cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeAccessToken([FromBody] RevokeAccessTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        IConvertToActionResult actionResult = result.ToActionResult(this);
        return actionResult.Convert();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.ToActionResult(this);
    }
}

