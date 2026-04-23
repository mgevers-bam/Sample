using Authentication.Core.Domain;
using Authentication.Infrastructure.Persistence;
using Authentication.Infrastructure.Persistence.Options;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Validation.AspNetCore;

namespace Authentication.Application.Api;

public static class OpenIddictConfiguration
{
    public static void ConfigureOpenIddict(WebApplicationBuilder builder, DatabaseOptions dbOptions)
    {
        // Register ASP.NET Core Identity
        builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<AuthenticationDbContext>()
            .AddDefaultTokenProviders();

        // Configure OpenIddict
        builder.Services.AddOpenIddict()
            .AddCore(options =>
            {
                // Configure OpenIddict to use the Entity Framework Core stores
                options.UseEntityFrameworkCore()
                    .UseDbContext<AuthenticationDbContext>();
            })
            .AddServer(options =>
            {
                // Enable the authorization, token, userinfo, and introspection endpoints
                options.SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetUserInfoEndpointUris("/connect/userinfo")
                    .SetIntrospectionEndpointUris("/connect/introspect")
                    .SetEndSessionEndpointUris("/connect/logout");

                // Enable the authorization code flow with PKCE
                options.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();

                // Enable the refresh token flow
                options.AllowRefreshTokenFlow();

                // Register the signing and encryption credentials
                // For development, use development certificates
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                // Disable access token encryption (use signed tokens only)
                options.DisableAccessTokenEncryption();

                // Register the ASP.NET Core host
                var aspNetCoreBuilder = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough();

                // Allow HTTP in development (disable HTTPS requirement)
                if (builder.Environment.IsDevelopment())
                {
                    aspNetCoreBuilder.DisableTransportSecurityRequirement();
                }
            })
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server
                options.UseLocalServer();

                // Register the ASP.NET Core host
                options.UseAspNetCore();
            });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });
    }
}
