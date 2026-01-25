using System.Diagnostics;
using System.Text;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate unique request ID
        var requestId = Guid.NewGuid().ToString();
        context.Items["RequestId"] = requestId;

        // Start timing
        var stopwatch = Stopwatch.StartNew();

        // Log request
        await LogRequest(context, requestId);

        // Capture original response body stream
        var originalResponseBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Call the next middleware in the pipeline
            await _next(context);

            // Stop timing before logging
            stopwatch.Stop();

            // Reset response body position to read from the beginning
            responseBody.Seek(0, SeekOrigin.Begin);

            // Log response
            await LogResponse(context, requestId, stopwatch.ElapsedMilliseconds);

            // Reset position again before copying
            responseBody.Seek(0, SeekOrigin.Begin);

            // Copy the response body back to the original stream
            await responseBody.CopyToAsync(originalResponseBodyStream);
        }
        finally
        {
            context.Response.Body = originalResponseBodyStream;
        }
    }

    private async Task LogRequest(HttpContext context, string requestId)
    {
        try
        {
            var request = context.Request;

            // Enable buffering to allow reading the request body multiple times
            request.EnableBuffering();

            var requestBody = string.Empty;
            if (request.ContentLength > 0 && request.ContentLength < 10000) // Only log if less than 10KB
            {
                using var reader = new StreamReader(
                    request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true);

                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0; // Reset stream position
            }

            _logger.LogInformation(
                "HTTP Request [{RequestId}] {Method} {Path} {QueryString} | Body: {Body}",
                requestId,
                request.Method,
                request.Path,
                request.QueryString,
                string.IsNullOrEmpty(requestBody) ? "(empty)" : requestBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging request");
        }
    }

    private async Task LogResponse(HttpContext context, string requestId, long elapsedMs)
    {
        try
        {
            var response = context.Response;

            var responseBody = string.Empty;
            if (response.Body.CanRead && response.Body.Length > 0 && response.Body.Length < 10000) // Only log if less than 10KB
            {
                using var reader = new StreamReader(response.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
                responseBody = await reader.ReadToEndAsync();
            }

            _logger.LogInformation(
                "HTTP Response [{RequestId}] {StatusCode} | Duration: {Duration}ms | Body: {Body}",
                requestId,
                response.StatusCode,
                elapsedMs,
                string.IsNullOrEmpty(responseBody) ? "(empty)" : responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging response");
        }
    }





}