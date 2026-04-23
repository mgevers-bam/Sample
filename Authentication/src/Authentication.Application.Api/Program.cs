using Authentication.Application.Api.Seeding;
using Authentication.Core.Contracts;
using Authentication.Infrastructure;
using Authentication.Infrastructure.Persistence;
using Authentication.Infrastructure.Persistence.Options;
using Common.Infrastructure.Auth.Options;
using Common.Infrastructure.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Authentication.Application.Api;

public class Program
{
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

        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
            ?? throw new InvalidOperationException($"{nameof(JwtOptions)} is missing.");

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
            .AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Program).Assembly))
            .AddScoped<ITokenService, TokenService>()
            .AddSingleton(jwtOptions);

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
    }
}
