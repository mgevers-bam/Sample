using Serilog.Context;

namespace Stargate.Api.OpenTelemetry;

public class RequestLogContext
{
    private readonly RequestDelegate _next;

    public RequestLogContext(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
        {
            return _next(context);
        }
    }
}
