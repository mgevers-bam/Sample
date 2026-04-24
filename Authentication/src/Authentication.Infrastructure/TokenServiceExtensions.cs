using Authentication.Core.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.Infrastructure;

public static class TokenServiceExtensions
{
    public static IServiceCollection AddTokenService(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new TokenServiceOptions
        {
            Issuer = configuration["Auth:Issuer"] ?? "http://authentication.api:5000/",
            AccessTokenLifetime = TimeSpan.FromHours(1),
            RefreshTokenLifetime = TimeSpan.FromDays(7)
        };

        services.AddSingleton(options);
        services.AddScoped<ITokenService, OpenIddictTokenService>();

        return services;
    }
}
