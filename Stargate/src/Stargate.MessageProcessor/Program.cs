using Common.Infrastructure.OpenTelemetry;
using Common.Infrastructure.ServiceBus.MassTransit;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Stargate.Core.Commands;
using Stargate.Persistence.Sql;
using Stargate.Persistence.Sql.Options;

namespace Stargate.MessageProcessor;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder
            .UseSerilog("stargate-message-processor", logToConsole: true, logToOtel: true)
            .SetupMassTransit(
                rabbitConfig: options => builder.Configuration.GetSection(nameof(RabbitMqTransportOptions)).Bind(options),
                configureBus: config =>
                {
                    config.AddConsumers(typeof(Program).Assembly);
                    config.AddConsumers(typeof(CreatePersonCommand).Assembly);
                });

        ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting Stargate Message Processor");

            if (!builder.Environment.IsEnvironment("Testing"))
            {
                var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<StargateDbContext>>();
                await EnsureDatabaseCreated(dbContextFactory, logger);
            }
        }

        await app.RunAsync();
    }

    private static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration, IHostEnvironment environment)
    {
        if (!environment.IsEnvironment("Testing"))
        {
            services.AddStargateRepositories(options =>
            {
                configuration.GetSection(nameof(StargateDbOptions)).Bind(options);
            });
        }

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreatePersonCommand).Assembly);
        });

        services.AddLogging();
        services.AddHttpClient();
    }

    private static async Task EnsureDatabaseCreated(IDbContextFactory<StargateDbContext> dbContextFactory, ILogger<Program> logger)
    {
        try
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            await dbContext.Database.EnsureCreatedAsync();
            await dbContext.Database.MigrateAsync();

            logger.LogInformation("Database created and migrated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating or migrating the database");
        }

    }
}
