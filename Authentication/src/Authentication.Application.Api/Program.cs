using Authentication.Application.Api.OnStartupHandlers;
using Authentication.Core.Commands;
using Authentication.Infrastructure;
using Authentication.Infrastructure.Persistence;
using Authentication.Infrastructure.Persistence.Options;
using Common.Infrastructure.Auth.Options;
using Common.Infrastructure.OpenTelemetry;
using Newtonsoft.Json;
using System.Reflection;

namespace Authentication.Application.Api;

public class Program
{
    protected Program() { }

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder);
        builder.Services.AddHostedService<ServiceStartup>();

        var app = builder.Build();
        ConfigureApplication(app);

        await app.RunAsync();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.UseSerilog("authentication-api", logToConsole: true, logToOtel: true);

        builder.Services
            .AddLogging()
            .AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

        builder.Services
            .AddEndpointsApiExplorer()
            .AddOpenApi();

        builder.Services
            .AddOpenApi()
            .AddPersistence(options =>
            {
                builder.Configuration.GetSection(nameof(DatabaseOptions)).Bind(options);
            })
            .AddMediatR(config =>
            {
                Assembly[] assemblies = [
                    typeof(Program).Assembly,
                    typeof(RegisterCommand).Assembly,
                ];
                config.RegisterServicesFromAssemblies(assemblies);
            });

        builder.Services.AddTokenService(options =>
        {
            builder.Configuration.GetSection(nameof(JwtOptions)).Bind(options);
        });

        OpenIddictConfiguration.ConfigureOpenIddict(builder, options =>
        {
            builder.Configuration.GetSection(nameof(JwtOptions)).Bind(options);
        });
    }

    private static void ConfigureApplication(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "Authentication API V1");
            });
        }

        app
            .UseHttpsRedirection()            
            .UseAuthentication()
            .UseAuthorization();

        app.MapControllers();
    }
}
