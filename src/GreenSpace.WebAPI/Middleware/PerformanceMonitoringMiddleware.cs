using System.Diagnostics;
/// <summary>
/// Middleware for monitoring API performance and detecting slow requests
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly long _warningThresholdMs;

    public PerformanceMonitoringMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMonitoringMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _warningThresholdMs = configuration.GetValue<long>("Performance:WarningThresholdMs", 3000);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = $"{context.Request.Method} {context.Request.Path}";
        var requestId = context.Items["RequestId"]?.ToString() ?? Guid.NewGuid().ToString();

        // Add headers BEFORE calling next middleware
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Response-Time-Ms"] = stopwatch.ElapsedMilliseconds.ToString();
            context.Response.Headers["X-Request-Id"] = requestId;
            return Task.CompletedTask;
        });

        try
        {
            // Execute the next middleware
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Log performance metrics
            if (elapsedMs > _warningThresholdMs)
            {
                _logger.LogWarning(
                    "SLOW REQUEST DETECTED | {RequestPath} | Duration: {Duration}ms | Status: {StatusCode} | User: {User}",
                    requestPath,
                    elapsedMs,
                    context.Response.StatusCode,
                    context.User?.Identity?.Name ?? "Anonymous");
            }
            else
            {
                _logger.LogInformation(
                    "Performance | {RequestPath} | Duration: {Duration}ms | Status: {StatusCode}",
                    requestPath,
                    elapsedMs,
                    context.Response.StatusCode);
            }
        }
    }
}