using Microsoft.Extensions.Logging;

namespace Stargate.Testing.Logging;

public record LogEntry(LogLevel LogLevel, string Message)
{
}
