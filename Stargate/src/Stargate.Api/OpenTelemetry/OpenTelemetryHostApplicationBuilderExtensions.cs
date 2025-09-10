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

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceInstanceId: Environment.MachineName)
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector();

        //builder.Logging.AddOpenTelemetry(config =>
        //{
        //    config.SetResourceBuilder(resourceBuilder);

        //    config.IncludeScopes = true;
        //    config.IncludeFormattedMessage = true;
        //});

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
                    .AddAspNetCoreInstrumentation()
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
