using Ardalis.Result;
using Authentication.Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Authentication.Core.Queries;

public class GetUserInfoQuery : IRequest<Result<UserInfoResponse>>
{
    public string UserId { get; set; } = string.Empty;
}

public class UserInfoResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public List<string> Roles { get; set; } = [];
}

public class GetUserInfoQueryHandler(
    ILogger<GetUserInfoQueryHandler> logger,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<GetUserInfoQuery, Result<UserInfoResponse>>
{
    public async Task<Result<UserInfoResponse>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            logger.LogInformation("User {UserId} not found.", request.UserId);
            return Result.NotFound("User not found");
        }

        var roles = await userManager.GetRolesAsync(user);

        var response = new UserInfoResponse
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            Roles = roles.ToList()
        };

        logger.LogInformation("User info retrieved for {UserName}.", user.UserName);
        return Result.Success(response);
    }
}
