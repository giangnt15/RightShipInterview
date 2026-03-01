using Microsoft.AspNetCore.Builder;

namespace RightShip.Core.Observability;

/// <summary>
/// Extension methods for configuring trace ID response header middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the X-Trace-Id header to responses for Grafana trace lookup.
    /// </summary>
    public static IApplicationBuilder UseTraceIdResponseHeader(this IApplicationBuilder app) =>
        app.UseMiddleware<TraceIdResponseMiddleware>();
}
