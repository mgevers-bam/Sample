using Authentication.Core.Boundary;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using Stargate.Core.Boundary;

namespace Authentication.Application.Api.Seeding;

public static class OpenIddictSeeder
{
    private static readonly IEnumerable<ScopeRecord> AllScopes = 
        AuthenticationScopes.Scopes
        .Concat(StargateScopes.Scopes);

    public static async Task SeedAsync(
        OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope> scopeManager,
        OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> applicationManager,
        CancellationToken cancellationToken)
    {
        await SeedScopesAsync(scopeManager, cancellationToken);
        await SeedApplicationsAsync(applicationManager, cancellationToken);
    }

    private static async Task SeedScopesAsync(
        OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope> scopeManager,
        CancellationToken cancellationToken)
    {
        var scopesDescriptors = AllScopes.Select(s =>
            {
                var descriptor = new OpenIddictScopeDescriptor
                {
                    Name = s.Name,
                    DisplayName = s.DisplayName,
                    Description = s.Description
                };
                // Add resources based on scope name (this is just an example, adjust as needed)
                foreach (var resource in s.Resources)
                {
                    descriptor.Resources.Add(resource);
                }

                return descriptor;
            })
            .ToList();

        // Create only the scopes that don't exist
        foreach (var descriptor in scopesDescriptors)
        {
            if (await scopeManager.FindByNameAsync(descriptor.Name!, cancellationToken) is null)
            {
                await scopeManager.CreateAsync(descriptor, cancellationToken);
            }
        }
    }

    private static async Task SeedApplicationsAsync(
        OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> applicationManager,
        CancellationToken cancellationToken)
    {
        if (await applicationManager.FindByClientIdAsync("stargate-app-client", cancellationToken) is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "stargate-app-client",
                DisplayName = "Stargate App Client",
                RedirectUris = { new Uri("http://localhost:3000/callback") },
                PostLogoutRedirectUris = { new Uri("http://localhost:3000") },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,

                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                    OpenIddictConstants.Permissions.ResponseTypes.Code,

                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api"
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                }
            }, cancellationToken);
        }

        // Add a public client for password flow (useful for testing and simple API clients)
        if (await applicationManager.FindByClientIdAsync("stargate-api-client", cancellationToken) is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "stargate-api-client",
                DisplayName = "Stargate API Client",
                ClientType = OpenIddictConstants.ClientTypes.Public,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,

                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "stargate.api"
                }
            }, cancellationToken);
        }
    }
}
