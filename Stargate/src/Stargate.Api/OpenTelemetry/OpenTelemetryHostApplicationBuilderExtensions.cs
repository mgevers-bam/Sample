using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Stargate.Api.OpenTelemetry;

public static class OpenTelemetryHostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder, string serviceName)
    {
        var tracingOtlpEndpoint = builder.Configuration["OTLP_ENDPOINT_URL"];
        var excludedPaths = builder.Configuration.GetSection("OpenTelemetry:ExcludedTracePaths").Get<string[]>() 
            ?? new[] { "/metrics", "/health", "/swagger", "/" };

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceInstanceId: Environment.MachineName)
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector();

        builder.Services
            .AddOpenTelemetry()
            .WithMetrics(meterBuilder => {
                meterBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter();
            })
            .WithTracing(tracingBuilder =>
            {
                tracingBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource("MassTransit")
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = (httpContext) =>
                        {
                            var path = httpContext.Request.Path.Value;
                            return !excludedPaths.Any(excludedPath => 
                                excludedPath == "/" ? path == "/" : path?.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase) == true);
                        };
                    })
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                if (tracingOtlpEndpoint != null)
                {
                    tracingBuilder.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
                    });
                }
                else
                {
                    tracingBuilder.AddConsoleExporter();
                }
            });

        builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
        builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
        builder.Services.AddMetrics();

        return builder;
    }
}
