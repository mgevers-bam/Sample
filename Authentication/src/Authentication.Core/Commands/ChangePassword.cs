using Ardalis.Result;
using Authentication.Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Authentication.Core.Commands;

public class ChangePasswordCommand : IRequest<Result>
{
    public string UserId { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordCommandHandler(
    ILogger<ChangePasswordCommandHandler> logger,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            logger.LogInformation("Password change failed: User {UserId} not found", request.UserId);
            return Result.NotFound("User not found");
        }

        // Validate current password
        var passwordValid = await userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!passwordValid)
        {
            logger.LogWarning("Password change failed: Invalid current password for user {UserName}", user.UserName);
            return Result.Unauthorized("Current password is incorrect");
        }

        // Validate new password
        if (request.NewPassword == request.CurrentPassword)
        {
            return Result.Error("New password must be different from current password");
        }

        // Change password
        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Password change failed for user {UserName}: {Errors}", user.UserName, errors);
            return Result.Error(errors);
        }

        logger.LogInformation("Password changed successfully for user {UserName}", user.UserName);
        return Result.Success();
    }
}
