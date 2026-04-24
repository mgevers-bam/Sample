using Authentication.Core.Contracts;
using Common.Infrastructure.Auth.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.Infrastructure;

public static class TokenServiceExtensions
{
    public static IServiceCollection AddTokenService(this IServiceCollection services, Action<JwtOptions> configure)
    {
        var options = new JwtOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddScoped<ITokenService, OpenIddictTokenService>();

        return services;
    }
}
