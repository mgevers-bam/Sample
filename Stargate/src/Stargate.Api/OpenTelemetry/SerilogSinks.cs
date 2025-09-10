namespace Stargate.Api.OpenTelemetry;

/// <summary>
/// Specifies which serilog sink to log events to.
/// </summary>
[Flags]
public enum SerilogSinks : short
{
    /// <summary>
    /// Log directly to MS SQL
    /// </summary>
    Sql = 1,
    /// <summary>
    /// Log directly to Open Search with a fallback to SQL
    /// </summary>
    OpenSearch = 2,
    /// <summary>
    /// Log to Console
    /// </summary>
    Console = 4,
    /// <summary>
    /// Log to Open Telemetry Exporter
    /// </summary>
    OpenTelemetry = 8,
}