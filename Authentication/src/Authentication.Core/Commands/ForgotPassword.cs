using Ardalis.Result;
using Authentication.Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Authentication.Core.Commands;

public class ForgotPasswordCommand : IRequest<Result<ForgotPasswordResponse>>
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordResponse
{
    public string Message { get; set; } = "If that email address is in our system, we have sent password reset instructions to it.";
}

public class ForgotPasswordCommandHandler(
    ILogger<ForgotPasswordCommandHandler> logger,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordResponse>>
{
    public async Task<Result<ForgotPasswordResponse>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Return success to prevent email enumeration attacks
            logger.LogInformation("Password reset requested for non-existent email: {Email}", request.Email);
            return Result.Success(new ForgotPasswordResponse());
        }

        // Generate reset token
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

        // Store reset token in a secure location (can be retrieved later for email sending)
        // The token is now available and should be sent via email through a separate service
        logger.LogInformation("Password reset token generated for user {UserId}", user.Id);

        return Result.Success(new ForgotPasswordResponse());
    }
}
