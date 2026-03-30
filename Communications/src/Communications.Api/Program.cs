using Common.Infrastructure.OpenTelemetry;
using Common.Infrastructure.OpenTelemetry.Enrichers;
using Newtonsoft.Json;
using Serilog;

namespace Communications.Api
{
    public partial class Program
    {
        protected Program()
        {
        }

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.UseSerilog("communications-api", logToConsole: true, logToOtel: true);
            ConfigureServices(builder.Services);

            var app = builder.Build();
            ConfigureApplication(app);

            await app.RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
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

            services.AddCors(options =>
            {
                options.AddPolicy("DevelopmentCors", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:4200", "http://localhost:5001", "http://localhost:5002")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });
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
        }
    }
}
