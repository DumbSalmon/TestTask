using System.Diagnostics;
using Npgsql;

namespace Nazarov_Test_Task.Middleware;

public class ApiLoggingAndExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiLoggingAndExceptionMiddleware> _logger;

    public ApiLoggingAndExceptionMiddleware(
        RequestDelegate next,
        ILogger<ApiLoggingAndExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);

            stopwatch.Stop();
            _logger.LogInformation(
                "HTTP {Method} {Path} returned {StatusCode} in {ElapsedMilliseconds} ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (PostgresException exception) when (exception.SqlState == "P0001")
        {
            stopwatch.Stop();
            _logger.LogWarning(
                exception,
                "PostgreSQL rejected request {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, exception.MessageText);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            _logger.LogError(
                exception,
                "Unhandled error during {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Произошла внутренняя ошибка сервера.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new { error = message });
    }
}