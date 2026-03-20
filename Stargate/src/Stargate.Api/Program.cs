using Common.Infrastructure.Auth;
using Common.Infrastructure.OpenTelemetry;
using Common.Infrastructure.ServiceBus.MassTransit;
using MassTransit;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using Stargate.Api.EventHandlers;
using Stargate.Api.Hubs;
using Stargate.Api.OpenTelemetry;
using Stargate.Api.Queries;
using Stargate.Core.Commands;
using Stargate.Infrastructure.ServerSentEvents;
using Stargate.Persistence.Sql;
using Stargate.Persistence.Sql.Options;

namespace Stargate.Api;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.UseSerilog("stargate-api", logToConsole: true, logToOtel: true);

        if (!builder.Environment.IsEnvironment("Testing"))
        {
            builder.SetupMassTransit(
                rabbitConfig: options => builder.Configuration.GetSection(nameof(RabbitMqTransportOptions)).Bind(options),
                configureBus: config =>
                {
                    config.AddRequestClient<CreatePersonCommand>();
                
                    config.AddConsumers(typeof(Program).Assembly);
                    config.AddConsumer<SendEventsToClientHandler>()
                        .Endpoint(e =>
                        {
                            e.Temporary = true;
                            e.InstanceId = Environment.MachineName;
                        });
                });
        }

        ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

        var app = builder.Build();
        ConfigureApplication(app);

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
            cfg.RegisterServicesFromAssembly(typeof(GetPeopleQuery).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(CreatePersonCommand).Assembly);
        });

        services
            .AddLogging()
            .AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

        services
            .AddEndpointsApiExplorer()
            .AddOpenApi();

        services.AddEventPublisher();

        services.AddCors(options =>
        {
            options.AddPolicy("DevelopmentCors", policy =>
            {
                policy
                    .WithOrigins("http://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        services.AddSignalR();
        services.AddJwtAuthentication("fake-domain", "fake-audience");
    }

    private static void ConfigureApplication(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseCors("DevelopmentCors");
            app.MapOpenApi();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "Stargate API V1");
            });
        }

        app.UseMiddleware<RequestLogContext>();
        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();

        app
            .UseAuthentication()
            .UseAuthorization();

        app.MapControllers();

        app.UseServerSentEvents("api/ServerSentEvents/stream");
        app.MapHub<ServerEventHub>("api/server-events-hub", options =>
        {
            options.Transports = HttpTransportType.WebSockets;
        });
    }
}
