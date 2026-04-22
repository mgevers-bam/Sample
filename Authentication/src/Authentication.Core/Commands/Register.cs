using Ardalis.Result;
using Authentication.Core.Domain;
using Common.LanguageExtensions.Utilities.ResultExtensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Authentication.Core.Commands;

public class RegisterCommand : IRequest<Result>
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterCommandHandler(
    ILogger<RegisterCommandHandler> logger,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<RegisterCommand, Result>
{
    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        { 
            UserName = request.UserName,
            Email = request.Email
        };

        var identityResult = await userManager.CreateAsync(user, request.Password);

        var result = identityResult.Succeeded
            ? Result.Success()
            : Result.Error(new ErrorList(identityResult.Errors.Select(e => e.Description)));

        return result
            .Tap(() => logger.LogInformation("User {UserName} registered successfully.", request.UserName))
            .TapError(error => logger.LogInformation("Registration failed: {Error}", error));
    }
}