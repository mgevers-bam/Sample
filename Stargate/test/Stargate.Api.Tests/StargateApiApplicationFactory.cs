using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stargate.Persistence.Sql;

namespace Stargate.Api.Tests;

public class StargateApiApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            ReplaceExistingSqlDatabaseWithSqlite(services);
        });
    }

    private static void ReplaceExistingSqlDatabaseWithSqlite(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StargateDbContext>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddDbContextFactory<StargateDbContext>(options =>
        {
            options.UseSqlite("Data Source=starbase.db");
        });
    }
}
