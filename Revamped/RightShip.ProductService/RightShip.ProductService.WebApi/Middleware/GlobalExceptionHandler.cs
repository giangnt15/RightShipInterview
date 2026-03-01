using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RightShip.ProductService.Domain.Exceptions;

namespace RightShip.ProductService.WebApi.Middleware;

/// <summary>
/// Global exception handler for unhandled exceptions. Returns consistent ProblemDetails (RFC 7807) responses.
/// </summary>
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (statusCode, title, detail) = exception switch
        {
            ProductNameRequiredException => (HttpStatusCode.BadRequest, "Invalid product", exception.Message),
            ProductPriceMustBeNonNegativeException => (HttpStatusCode.BadRequest, "Invalid product", exception.Message),
            ProductQuantityMustBeNonNegativeException => (HttpStatusCode.BadRequest, "Invalid product", exception.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred",
                env.IsDevelopment() ? exception.ToString() : "An error occurred while processing your request.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.com/{(int)statusCode}"
        };

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
