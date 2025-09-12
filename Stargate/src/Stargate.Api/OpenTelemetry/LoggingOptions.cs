using System.ComponentModel.DataAnnotations;

namespace Stargate.Api.OpenTelemetry;

public class LoggingOptions
{
    [Required]
    public Serilog.Events.LogEventLevel LogLevel { get; set; } = Serilog.Events.LogEventLevel.Information;

    public string LogName { get; set; } = "Stargate-Log";
    [Required]
    public string LoggingConnection { get; set; }

    [Required]
    public SerilogSinks LogSink { get; set; } = SerilogSinks.OpenTelemetry;

    [Required]
    public string FallbackLoggingDbConnection { get; set; } = string.Empty;
}