using Duende.IdentityServer.Models;

namespace Authentication.Application.Api.Seeding;

public static class IdentityServerSeedData
{
    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
        };
    }

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return new List<ApiScope>
        {
            new ApiScope("api", "Golf App API"),
        };
    }

    public static IEnumerable<ApiResource> GetApiResources()
    {
        return new List<ApiResource>
        {
            new ApiResource("api", "Golf App API")
            {
                Scopes = { "api" },
                UserClaims = { "role" }
            },
        };
    }

    public static IEnumerable<Client> GetClients()
    {
        return new List<Client>
        {
            new Client
            {
                ClientId = "golf-app-client",
                ClientName = "Golf App Client",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RedirectUris = { "http://localhost:3000/callback" },
                AllowedCorsOrigins = { "http://localhost:3000" },
                AllowedScopes = { "openid", "profile", "email", "api" },
                RequirePkce = true,
                AllowAccessTokensViaBrowser = true,
            },
        };
    }
}
