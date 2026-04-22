using Ardalis.Result;
using Authentication.Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Authentication.Core.Commands;

public class LogoutCommand : IRequest<Result>
{
    public string UserId { get; set; } = string.Empty;
}

public class LogoutCommandHandler(
    ILogger<LogoutCommandHandler> logger,
    SignInManager<ApplicationUser> signInManager)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await signInManager.SignOutAsync();
            logger.LogInformation("User {UserId} logged out successfully.", request.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Logout failed for user {UserId}.", request.UserId);
            return Result.Error("Logout failed");
        }
    }
}
