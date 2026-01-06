using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;

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
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                });

                // Create a logging wrapper around the configuration manager
                var baseRetriever = new OpenIdConnectConfigurationRetriever();
                var loggingRetriever = new LoggingConfigurationRetriever(baseRetriever);
                
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    openIdEndpoint,
                    loggingRetriever,
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
                    ValidateIssuerSigningKey = true,
                };
            });

        return services;
    }

    private class LoggingConfigurationRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
    {
        private readonly IConfigurationRetriever<OpenIdConnectConfiguration> _innerRetriever;

        public LoggingConfigurationRetriever(IConfigurationRetriever<OpenIdConnectConfiguration> innerRetriever)
        {
            _innerRetriever = innerRetriever;
        }

        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
        {
            Log.Information("Retrieving OpenID Connect configuration from: {Address}", address);
            
            var config = await _innerRetriever.GetConfigurationAsync(address, retriever, cancel);
            
            Log.Information("Successfully retrieved configuration. Signing keys count: {KeyCount}, Issuer: {Issuer}", 
                config.SigningKeys?.Count ?? 0, config.Issuer);
            
            if (config.SigningKeys != null && config.SigningKeys.Any())
            {
                foreach (var key in config.SigningKeys)
                {
                    Log.Information("Signing key retrieved - KeyId: {KeyId}, Algorithm: {Algorithm}", 
                        key.KeyId ?? "N/A", 
                        (key as SecurityKey)?.GetType().Name ?? "Unknown");
                }
            }
            
            return config;
        }
    }
}
