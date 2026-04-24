using Common.Infrastructure.OpenTelemetry;
using Common.Infrastructure.ServiceBus.MassTransit;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Stargate.Core.Commands;
using Stargate.Persistence.Sql;
using Stargate.Persistence.Sql.Options;

namespace Stargate.MessageProcessor;

public partial class Program
{
    protected Program() { }

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
        builder.Services.AddHostedService<ServiceStartup>();

        await builder.Build().RunAsync();
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
}
