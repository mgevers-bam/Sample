using Authentication.Infrastructure.Persistence.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication.Infrastructure.Persistence;

public static class PeristenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DatabaseOptions> configure)
    {
        var options = new DatabaseOptions();
        configure(options);

        return services
            .AddDbContext<AuthenticationDbContext>(dbOptions =>
            {
                dbOptions.UseSqlServer(options.AuthConnectionString, npgOptions =>
                {
                    npgOptions
                        .MigrationsAssembly(typeof(AuthenticationDbContext).Assembly.GetName().Name)
                        .MigrationsHistoryTable("__EFMigrationsHistory");
                });
            });
    }
}
