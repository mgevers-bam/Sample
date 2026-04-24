using Ardalis.Result;
using Authentication.Core.Contracts;
using Authentication.Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Authentication.Core.Commands;

public class LoginCommand : IRequest<Result<TokenResponse>>
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public IEnumerable<string> Scopes { get; set; } = [];
}

public class LoginCommandHandler(
    ILogger<LoginCommandHandler> logger,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService)
    : IRequestHandler<LoginCommand, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await signInManager.UserManager.FindByNameAsync(request.UserName);
        if (user is null)
        {
            logger.LogInformation("Login failed: User {UserName} not found.", request.UserName);
            return Result.Unauthorized();
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            logger.LogInformation("Login failed: Invalid password for user {UserName}.", request.UserName);
            return Result.Unauthorized();
        }

        logger.LogInformation("User {UserName} logged in successfully.", request.UserName);

        var tokenResponse = await tokenService.GenerateTokensAsync(user, request.Scopes, cancellationToken);
        return Result.Success(tokenResponse);
    }
}
