using Authentication.Application.Api.Seeding;
using Authentication.Core.Commands;
using Authentication.Core.Contracts;
using Authentication.Infrastructure;
using Authentication.Infrastructure.Persistence;
using Authentication.Infrastructure.Persistence.Options;
using Common.Infrastructure.OpenTelemetry;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Cryptography;

namespace Authentication.Application.Api;

public class Program
{
    protected Program() { }

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder);

        var app = builder.Build();
        ConfigureApplication(app);

        await RunMigrations(app.Services);
        await app.RunAsync();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        var dbOptions = builder.Configuration.GetSection(nameof(DatabaseOptions)).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"{nameof(DatabaseOptions)} is missing.");

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
            .AddPersistence(dbOptions)
            .AddMediatR(config =>
            {
                Assembly[] assemblies = [
                    typeof(Program).Assembly,
                    typeof(RegisterCommand).Assembly,
                ];
                config.RegisterServicesFromAssemblies(assemblies);
            });

        // Register token service
        builder.Services.AddTokenService(builder.Configuration);

        OpenIddictConfiguration.ConfigureOpenIddict(builder, dbOptions);
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

    private static async Task RunMigrations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
        if (!await db.Database.CanConnectAsync())
        {
            await db.Database.EnsureCreatedAsync();
        }

        // Seed OpenIddict configuration data
        await OpenIddictSeeder.SeedAsync(scope.ServiceProvider);

        // Seed test user
        await TestUserSeeder.SeedAsync(scope.ServiceProvider);
    }
}
