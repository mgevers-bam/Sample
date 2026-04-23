using OpenIddict.Abstractions;

namespace Authentication.Application.Api.Seeding;

public static class OpenIddictSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var scopeManager = serviceProvider.GetRequiredService<IOpenIddictScopeManager>();
        var applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Seed standard OpenID Connect scopes
        await SeedScopesAsync(scopeManager);

        // Seed client applications
        await SeedApplicationsAsync(applicationManager);
    }

    private static async Task SeedScopesAsync(IOpenIddictScopeManager scopeManager)
    {
        // Standard OpenID Connect scopes
        if (await scopeManager.FindByNameAsync("openid") is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "openid",
                DisplayName = "OpenID",
                Description = "Grants access to the OpenID Connect identity token"
            });
        }

        if (await scopeManager.FindByNameAsync("profile") is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "profile",
                DisplayName = "Profile",
                Description = "Grants access to profile information (name, etc.)"
            });
        }

        if (await scopeManager.FindByNameAsync("email") is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "email",
                DisplayName = "Email",
                Description = "Grants access to email address"
            });
        }

        var stargateScope = await scopeManager.FindByNameAsync("stargate.api");
        if (stargateScope is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "stargate.api",
                DisplayName = "Stargate App API",
                Description = "Grants access to the Stargate App API",
                Resources = { "stargate.api" }
            });
        }
    }

    private static async Task SeedApplicationsAsync(IOpenIddictApplicationManager applicationManager)
    {
        if (await applicationManager.FindByClientIdAsync("stargate-app-client") is null)
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
            });
        }
    }
}
