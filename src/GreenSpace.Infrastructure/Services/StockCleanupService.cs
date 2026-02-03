using GreenSpace.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Infrastructure.Services
{
    public class StockCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StockCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Run every 5 minutes
        public StockCleanupService(
            IServiceProvider serviceProvider,
            ILogger<StockCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stock Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_interval, stoppingToken);

                    using var scope = _serviceProvider.CreateScope();
                    var stockService = scope.ServiceProvider
                        .GetRequiredService<IStockService>();

                    var result = await stockService.CleanupExpiredReservationsAsync(stoppingToken);

                    if (result.IsSuccess && result.Data > 0)
                    {
                        _logger.LogInformation(
                            "Cleaned up {Count} expired stock reservations",
                            result.Data);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Stock Cleanup Service");
                }
            }

            _logger.LogInformation("Stock Cleanup Service stopped");
        }
    }
}
