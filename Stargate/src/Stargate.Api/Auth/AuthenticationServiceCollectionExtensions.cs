using Microsoft.AspNetCore.Authentication.JwtBearer;
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
                options.Authority = authOptions.Authority;
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
