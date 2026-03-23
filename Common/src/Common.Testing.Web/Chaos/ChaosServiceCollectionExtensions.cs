using Common.Infrastructure.Mediator;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Common.Testing.Integration.Chaos;

public static class ChaosServiceCollectionExtensions
{
    public static IServiceCollection AddMediatRChaos(
        this IServiceCollection services,
        Assembly assembly)
    {
        return services
            .AddSingleton<ChaosRequestManager>()
            .AddMediatR(configuration =>
            {
                //configuration.AddOpenBehavior(typeof(RetryBehavior<,>));
                configuration.AddOpenBehavior(typeof(ChaosPipelineBehavior<,>));

                configuration.RegisterServicesFromAssembly(typeof(ChaosServiceCollectionExtensions).Assembly);
            });
    }

    public static IServiceCollection AddMediatRChaos(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        return services
            .AddSingleton<ChaosRequestManager>()
            .AddMediatR(configuration =>
            {
                //configuration.AddOpenBehavior(typeof(RetryBehavior<,>));
                configuration.AddOpenBehavior(typeof(ChaosPipelineBehavior<,>));
                var allAssemblies = assemblies
                    .Concat([typeof(ChaosServiceCollectionExtensions).Assembly])
                    .ToArray();

                configuration.RegisterServicesFromAssemblies(allAssemblies);
            });
    }
}
