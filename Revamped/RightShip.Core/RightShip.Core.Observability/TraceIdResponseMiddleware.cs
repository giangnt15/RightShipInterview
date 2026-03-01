using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace RightShip.Core.Observability;

/// <summary>
/// Adds the current trace ID to response headers so clients can search for traces in Grafana.
/// </summary>
public class TraceIdResponseMiddleware
{
    private const string TraceIdHeader = "X-Trace-Id";
    private readonly RequestDelegate _next;

    public TraceIdResponseMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.TraceId.ToString();
        if (!string.IsNullOrEmpty(traceId))
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[TraceIdHeader] = traceId;
                return Task.CompletedTask;
            });
        }
        await _next(context);
    }
}
