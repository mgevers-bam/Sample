using Common.LanguageExtensions.DependencyInjection;
using Common.Testing.Web;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stargate.Persistence.Repositories;
using Stargate.Persistence.Sql;

namespace Stargate.Api.Tests;

public class StargateApiApplicationFactory : WebAppFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName = $"starbase-{Guid.NewGuid()}.db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.AddDbContextFactory<StargateDbContext>(options =>
            {
                options.UseSqlite($"Data Source={_databaseName}");
            });

            services.AddScopedAsAllImplementedInterfaces<PersonRepository>();
            //services.AddMassTransitTestHarness();
        });
    }

    public async ValueTask InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<StargateDbContext>>();

        using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SqliteConnection.ClearAllPools();

            // Delete the database file
            if (File.Exists(_databaseName))
            {
                File.Delete(_databaseName);
            }
        }

        base.Dispose(disposing);
    }
}
