using Authentication.Core.Domain;
using Authentication.Infrastructure.Persistence;
using Authentication.Infrastructure.Persistence.Options;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Application.Api;

public static class DuendeConfiguration
{
    public static void ConfigureIdentityStore(WebApplicationBuilder builder, DatabaseOptions dbOptions)
    {
        // Register ASP.NET Core Identity
        builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<AuthenticationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.KeyManagement.Enabled = false;
            })
            .AddAspNetIdentity<ApplicationUser>()
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b =>
                    b.UseSqlServer(dbOptions.ConfigStoreConnectionString,
                        sql => sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b =>
                    b.UseSqlServer(dbOptions.OperationStoreConnectionString,
                        sql => sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
            })
            .AddDeveloperSigningCredential(); // TODO: Replace with production signing credentials

        builder.Services.AddAuthentication()
            .AddOpenIdConnect("oidc", "Sign-in with demo.duendesoftware.com", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                options.SaveTokens = true;

                options.Authority = "https://demo.duendesoftware.com";
                options.ClientId = "interactive.confidential";
                options.ClientSecret = "secret";
                options.ResponseType = "code";

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });
    }
}
