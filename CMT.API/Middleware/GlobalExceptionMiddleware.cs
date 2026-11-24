// ============================================================
//  CMT.API – Global Exception Middleware
//  Returns consistent JSON error envelopes for all unhandled exceptions
// ============================================================
using System.Net;
using System.Text.Json;

namespace CMT.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        (int statusCode, string message) = ex switch
        {
            KeyNotFoundException  _  => (404, ex.Message),
            UnauthorizedAccessException _ => (401, ex.Message),
            ArgumentException     _  => (400, ex.Message),
            InvalidOperationException _ when ex.Message.Contains("Insufficient stock")
                                         => (409, ex.Message),
            InvalidOperationException _ => (422, ex.Message),
            _                           => (500, "An unexpected error occurred. Please try again.")
        };

        context.Response.StatusCode = statusCode;

        var payload = JsonSerializer.Serialize(new
        {
            success   = false,
            statusCode,
            message,
            timestamp = DateTime.UtcNow
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return context.Response.WriteAsync(payload);
    }
}
