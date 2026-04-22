using Ardalis.Result;
using Authentication.Core.Contracts;
using Authentication.Core.Domain;
using Common.LanguageExtensions.Utilities.ResultExtensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace Authentication.Core.Commands;

public class LoginCommand : IRequest<Result<LoginResponse>>
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
}

public class LoginCommandHanlder(
    ILogger<LoginCommandHanlder> logger,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    JwtSecurityTokenHandler tokenHandler)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await signInManager.UserManager.FindByNameAsync(request.UserName);
        if (user == null)
        {
            logger.LogInformation("Login failed: User {UserName} not found.", request.UserName);
            return Result.Unauthorized("Invalid Credentials");
        }

        var signInResult = await signInManager.PasswordSignInAsync(user, request.Password, false, false);

        var result = signInResult.Succeeded
            ? Result.Success()
            : Result.Unauthorized("Invalid Credentials");

        return await result
            .Tap(() => logger.LogInformation("User {UserName} logged in successfully.", request.UserName))
            .TapError(error => logger.LogInformation("Login failed: {Error}", error))
            .Map<LoginResponse>(async () =>
            {
                var accessToken = await tokenService.GenerateAccessToken(user);
                var refreshToken = await tokenService.GenerateRefreshToken(user);

                return new LoginResponse
                {
                    AccessToken = tokenHandler.WriteToken(accessToken),
                    RefreshToken = refreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = (int)Math.Ceiling(accessToken.ValidTo.Subtract(DateTime.UtcNow).TotalSeconds)
                };
            });
    }
}