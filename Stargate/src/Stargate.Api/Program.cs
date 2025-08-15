using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stargate.Api.Queries;
using Stargate.Core.Commands;
using Stargate.Persistence.Sql;
using Stargate.Persistence.Sql.Options;
using Stargate.Api.OpenTelemetry;
using Serilog;

namespace Stargate.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder
                .UseSerilog(logToConsole: true, logToOtel: true)
                .ConfigureOpenTelemetry(serviceName: "stargate-api");

            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();
            ConfigureApplication(app);

            using (var scope = app.Services.CreateScope())
            {
                var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<StargateDbContext>>();
                await EnsureDatabaseCreated(dbContextFactory);
            }

            await app.RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddStargateRepositories(options =>
            {
                configuration.GetSection(nameof(StargateDbOptions)).Bind(options);
            });

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
                .AddSwaggerGen();
        }

        private static void ConfigureApplication(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app
                    .UseSwagger()
                    .UseSwaggerUI();
            }

            app.UseMiddleware<RequestLogContext>();
            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
        }

        private static async Task EnsureDatabaseCreated(IDbContextFactory<StargateDbContext> dbContextFactory)
        {
            using var dbContext = await dbContextFactory.CreateDbContextAsync();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
            await dbContext.Database.MigrateAsync();
        }
    }
}
