using Authentication.Application.Api.Seeding;
using Authentication.Core.Domain;
using Authentication.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

namespace Authentication.Application.Api.OnStartupHandlers;

public class SeedAuthDataRequest : IRequest
{
}

public class SeedAuthDataHandler(
    AuthenticationDbContext dbContext,
    OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope> scopeManager,
    OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> applicationManager,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ILogger<SeedAuthDataHandler> logger) 
    : IRequestHandler<SeedAuthDataRequest>
{
    public async Task Handle(SeedAuthDataRequest request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Starting to seed auth data.");

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        await OpenIddictSeeder.SeedAsync(scopeManager, applicationManager, cancellationToken);
        await TestUserSeeder.SeedAsync(userManager, roleManager);
    }
}
