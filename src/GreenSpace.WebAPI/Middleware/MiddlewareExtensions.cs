

namespace GreenSpace.WebAPI.Middleware
{
    /// <summary>
    /// Extension methods for registering middleware
    /// </summary>
    public static class MiddlewareExtensions
    {

        /// <summary>
        /// Registers global exception handling middleware
        /// </summary>
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }

        /// <summary>
        /// Uses the request logging.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }

        /// <summary>
        /// Registers performance monitoring middleware
        /// </summary>
        public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder app)
        {
            return app.UseMiddleware<PerformanceMonitoringMiddleware>();
        }

        public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
        {
            // Order matters! Exception handling should be first
            app.UseExceptionHandling();
            app.UsePerformanceMonitoring();
            app.UseRequestLogging();

            return app;
        }
    }
}