using Ardalis.Result;
using Authentication.Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Authentication.Core.Commands;

public class ResetPasswordCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPasswordCommandHandler(
    ILogger<ResetPasswordCommandHandler> logger,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            logger.LogWarning("Password reset attempted for non-existent email: {Email}", request.Email);
            return Result.Error("Invalid email or reset token");
        }

        try
        {
            var identityResult = await userManager.ResetPasswordAsync(user, request.ResetToken, request.NewPassword);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errors);
                return Result.Error(new ErrorList(identityResult.Errors.Select(e => e.Description)));
            }

            logger.LogInformation("Password reset successful for user {UserId}", user.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred during password reset for user {UserId}", user.Id);
            return Result.Error("An error occurred while resetting password. Please try again.");
        }
    }
}
