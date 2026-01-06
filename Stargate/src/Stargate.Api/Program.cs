using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stargate.Api.Queries;
using Stargate.Core.Commands;
using Stargate.Persistence.Sql;
using Stargate.Persistence.Sql.Options;
using Stargate.Api.OpenTelemetry;
using Serilog;
using Stargate.Api.Auth;

namespace Stargate.Api
{
    public static partial class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var loggingOptions = builder.Configuration.GetSection(nameof(LoggingOptions))
                .Get<LoggingOptions>() ?? throw new InvalidOperationException("LoggingOptions section is missing in configuration.");

            builder
                .UseMyVectorLogging(loggingOptions!)
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

            services.AddJwtAuthenticationServices(options =>
            {
                configuration.GetSection(nameof(AuthOptions)).Bind(options);
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
                .AddSwaggerGen(options =>
                {
                    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                        Description = "Enter your JWT token in the format: {token}"
                    });

                    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                    {
                        {
                            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                            {
                                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                                {
                                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });
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

            app
                .UseAuthentication()
                .UseAuthorization();

            app.MapControllers();
            app.MapPrometheusScrapingEndpoint();
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
