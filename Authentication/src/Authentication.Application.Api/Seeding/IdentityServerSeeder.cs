using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Application.Api.Seeding;

public static class IdentityServerSeeder
{
    public static async Task SeedAsync(ConfigurationDbContext context)
    {
        var identityResouces = await context.IdentityResources.ToListAsync();
        var apiScopes = await context.ApiScopes.ToListAsync();
        var apiResources = await context.ApiResources.ToListAsync();
        var clients = await context.Clients.ToListAsync();

        if (identityResouces.Count == 0)
        {
            foreach (var resource in IdentityServerSeedData.GetIdentityResources())
            {
                context.IdentityResources.Add(resource.ToEntity());
            }
        }

        if (apiScopes.Count == 0)
        {
            foreach (var scope in IdentityServerSeedData.GetApiScopes())
            {
                context.ApiScopes.Add(scope.ToEntity());
            }
        }

        if (apiResources.Count == 0)
        {
            foreach (var resource in IdentityServerSeedData.GetApiResources())
            {
                context.ApiResources.Add(resource.ToEntity());
            }
        }

        if (clients.Count == 0)
        {
            foreach (var client in IdentityServerSeedData.GetClients())
            {
                context.Clients.Add(client.ToEntity());
            }
        }

        await context.SaveChangesAsync();
    }
}
