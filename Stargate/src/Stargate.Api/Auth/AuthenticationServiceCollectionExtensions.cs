using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Stargate.Api.Auth;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthenticationServices(this IServiceCollection services, Action<AuthOptions> configureOptions)
    {
        var authOptions = new AuthOptions();
        configureOptions(authOptions);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var localUrl = authOptions.Authority.Replace("localhost", "host.docker.internal");
                var openIdEndpoint = $"{localUrl.TrimEnd('/')}/.well-known/openid-configuration";

                // Create HTTP client that accepts self-signed certificates for development
                var httpClient = new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    openIdEndpoint,
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever(httpClient));

                options.Authority = authOptions.Authority;
                options.RequireHttpsMetadata = false; // for development with self-signed certs
                options.MetadataAddress = openIdEndpoint;
                options.ConfigurationManager = configurationManager;
                
                // Configure backchannel to use same SSL bypass for ongoing metadata refresh
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                options.BackchannelTimeout = TimeSpan.FromSeconds(30);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuer = true,
                    ValidIssuer = authOptions.Authority.TrimEnd('/'),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    ValidateAudience = false,
                    ValidTypes = new[] { "at+jwt" },
                    ValidateIssuerSigningKey = true
                };
            });

        return services;
    }
}
