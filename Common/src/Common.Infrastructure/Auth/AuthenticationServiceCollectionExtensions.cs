using Common.Infrastructure.Auth.ApiKey;
using Common.Infrastructure.Auth.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Serilog;
using System.Diagnostics;
using System.Text;

namespace Common.Infrastructure.Auth;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAndAPIKeyAuthentication(this IServiceCollection services, string domain, IReadOnlyCollection<string> audiences)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = "JwtOrApiKey";
                options.DefaultChallengeScheme = "JwtOrApiKey";
            })
            .AddJwtBearer("Bearer", delegate (JwtBearerOptions c)
            {
                c.Authority = "https://" + domain;
                c.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudiences = audiences,
                    ValidIssuer = domain,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ClockSkew = TimeSpan.FromMinutes(5.0),
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateLifetime = true
                };
            })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.ApiKeyAuthenticationScheme, configureOptions: null)
            .AddPolicyScheme("JwtOrApiKey", JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    string authorization = context.Request.Headers[HeaderNames.Authorization]!;
                    if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                    {
                        return JwtBearerDefaults.AuthenticationScheme;
                    }
                    return ApiKeyAuthenticationHandler.ApiKeyAuthenticationScheme;
                };
            });

        return services;
    }

    public static IServiceCollection AddJwtAndAPIKeyAuthentication(this IServiceCollection services, string domain, string audience)
    {
        return services.AddJwtAndAPIKeyAuthentication(domain, new[] { audience });
    }

    public static IServiceCollection AddJwtAndAPIKeyAuthentication(this IServiceCollection services, Action<OAuthOptions> configure)
    {
        var options = new OAuthOptions();
        configure(options);

        return services.AddJwtAndAPIKeyAuthentication(domain: options.Domain, audience: options.Audience);
    }

    public static IServiceCollection AddAuth0JwtAuthentication(this IServiceCollection services, string domain, string audience)
    {
        return services.AddAuth0JwtAuthentication(domain, [audience]);
    }

    public static IServiceCollection AddAuth0JwtAuthentication(this IServiceCollection services, string domain, IReadOnlyCollection<string> audiences)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("Bearer", delegate (JwtBearerOptions c)
            {
                c.Authority = "https://" + domain;
                c.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudiences = audiences,
                    ValidIssuer = domain,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ClockSkew = TimeSpan.FromMinutes(5.0),
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateLifetime = true
                };
            });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        string authorityUrl,
        string issuer,
        string audience)
    {
        var openIdUrl = $"{authorityUrl.TrimEnd('/')}/.well-known/openid-configuration";

        // Create a logging HTTP handler to trace all OIDC discovery requests
        var loggingHandler = new LoggingHttpMessageHandler(new HttpClientHandler());
        var httpClient = new HttpClient(loggingHandler);
        var documentRetriever = new HttpDocumentRetriever(httpClient)
        {
            RequireHttps = false // Allow HTTP for local development
        };

        // Log initial discovery attempt
        Log.Information("Configuring JWT authentication with Authority: {Authority}, Issuer: {Issuer}, Audience: {Audience}",
            authorityUrl, issuer, audience);
        Log.Information("OpenID Configuration URL: {OpenIdUrl}", openIdUrl);

        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            openIdUrl,
            new OpenIdConnectConfigurationRetriever(),
            documentRetriever);

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("Bearer", c =>
            {
                c.Authority = authorityUrl;
                c.ConfigurationManager = configManager;
                c.MetadataAddress = openIdUrl;
                c.RequireHttpsMetadata = false; // Allow HTTP for local development

                // Use the same logging HTTP client for backchannel
                c.BackchannelHttpHandler = new LoggingHttpMessageHandler(new HttpClientHandler());

                c.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidTypes = ["at+jwt"],
                    ValidateIssuerSigningKey = true
                };

                c.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = async context =>
                    {
                        Log.Warning(context.Exception, "JWT authentication failed: {ErrorMessage}", context.Exception.Message);

                        // Log additional details for signature validation failures
                        if (context.Exception.Message.Contains("IDX10500"))
                        {
                            Log.Warning("Signature validation failed - attempting to diagnose...");

                            // Try to get the current configuration to see what keys are loaded
                            try
                            {
                                var config = await context.Options.ConfigurationManager!.GetConfigurationAsync(CancellationToken.None);
                                Log.Warning("OIDC Configuration loaded. Issuer: {Issuer}, JWKS URI: {JwksUri}, SigningKeys count: {KeyCount}",
                                    config.Issuer, config.JwksUri, config.SigningKeys?.Count ?? 0);

                                if (config.SigningKeys != null)
                                {
                                    foreach (var key in config.SigningKeys)
                                    {
                                        Log.Warning("  SigningKey: KeyId={KeyId}, Type={KeyType}", key.KeyId, key.GetType().Name);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Failed to retrieve OIDC configuration for diagnostics");
                            }
                        }
                    },
                    OnTokenValidated = context =>
                    {
                        var claims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}");
                        Log.Information("JWT token validated successfully. Claims: {Claims}", string.Join(", ", claims ?? []));
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Log.Warning("JWT challenge issued. Error: {Error}, ErrorDescription: {ErrorDescription}",
                            context.Error, context.ErrorDescription);
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var hasToken = !string.IsNullOrEmpty(context.Token) ||
                            context.Request.Headers.ContainsKey("Authorization");
                        Log.Debug("JWT message received. Has token: {HasToken}", hasToken);
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    /// <summary>
    /// HTTP message handler that logs all requests and responses for debugging OIDC discovery issues.
    /// </summary>
    private class LoggingHttpMessageHandler : DelegatingHandler
    {
        public LoggingHttpMessageHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            Log.Information("[OIDC HTTP] --> {Method} {Uri}", request.Method, request.RequestUri);

            // Log request headers
            foreach (var header in request.Headers)
            {
                Log.Debug("[OIDC HTTP] Request Header: {Name}={Value}", header.Key, string.Join(", ", header.Value));
            }

            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log.Error(ex, "[OIDC HTTP] <-- FAILED {Method} {Uri} after {ElapsedMs}ms: {Error}",
                    request.Method, request.RequestUri, stopwatch.ElapsedMilliseconds, ex.Message);
                throw;
            }

            stopwatch.Stop();

            Log.Information("[OIDC HTTP] <-- {StatusCode} {Method} {Uri} ({ElapsedMs}ms)",
                (int)response.StatusCode, request.Method, request.RequestUri, stopwatch.ElapsedMilliseconds);

            // Log response headers
            foreach (var header in response.Headers)
            {
                Log.Debug("[OIDC HTTP] Response Header: {Name}={Value}", header.Key, string.Join(", ", header.Value));
            }

            // Log response body for OIDC endpoints (they're small JSON responses)
            if (request.RequestUri?.PathAndQuery.Contains(".well-known") == true ||
                request.RequestUri?.PathAndQuery.Contains("jwks") == true)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                // Truncate if too long
                var truncatedContent = content.Length > 2000 ? content[..2000] + "... [truncated]" : content;
                Log.Information("[OIDC HTTP] Response Body: {Body}", truncatedContent);

                // Check if the response looks like a JWT instead of JSON
                if (content.Split('.').Length == 3 && !content.StartsWith("{"))
                {
                    Log.Error("[OIDC HTTP] WARNING: Response appears to be a JWT token instead of JSON! " +
                        "This indicates the JWKS endpoint is misconfigured and returning a token instead of keys.");
                }
            }

            return response;
        }
    }


    public static IServiceCollection AddAuth0JwtAuthentication(this IServiceCollection services, Action<OAuthOptions> configure)
    {
        var options = new OAuthOptions();
        configure(options);

        return services.AddAuth0JwtAuthentication(domain: options.Domain, audience: options.Audience);
    }
}
