using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Stargate.Persistence.Repositories;
using Stargate.Persistence.Sql.Options;

namespace Stargate.Persistence.Sql;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddStargateDbContext(this IServiceCollection services, Action<StargateDbOptions> config)
    {
        services
           .AddOptions<StargateDbOptions>()
           .Configure(config);

        services
                .AddDbContextFactory<StargateDbContext>((serviceProvider, builder) =>
                {
                    var sqlOptions = serviceProvider.GetRequiredService<IOptions<StargateDbOptions>>();

                    builder.UseSqlServer(sqlOptions.Value.ConnectionString, options =>
                    {
                        options.MigrationsAssembly(typeof(StargateDbContext).Assembly.GetName().Name);
                        options.MigrationsHistoryTable($"__{nameof(StargateDbContext)}");

                        options.EnableRetryOnFailure(5);
                        options.MinBatchSize(1);
                    });
                })
                .AddDbContext<StargateDbContext>();

        return services;
    }

    public static IServiceCollection AddStargateRepositories(this IServiceCollection services, Action<StargateDbOptions> config)
    {
        services.AddStargateDbContext(config);

        services
            .AddScopedAsAllImplementedInterfaces<PersonRepository>();
            //.AddScoped<IRepository<AstronautDuty>, EFRepository<AstronautDuty>>()
            //.AddScoped<IRepository<AstronautDetail>, EFRepository<AstronautDetail>>();

        return services;
    }
}
