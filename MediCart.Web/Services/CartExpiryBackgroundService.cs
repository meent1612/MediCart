using MediCart.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace MediCart.Web.Services
{
    public class CartExpiryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CartExpiryBackgroundService> _logger;

        // How often the background service wakes up and checks for expired items.
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

        public CartExpiryBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<CartExpiryBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cart expiry background service started.");

            // Graceful execution loop - runs until host cancellation is requested.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ReleaseExpiredItemsAsync();

                    // Wait 1 hour before checking again.
                    await Task.Delay(CheckInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break; // expected on shutdown — exit cleanly
                }
                catch (Exception ex)
                {
                    // Log but don't crash — the service will try again next hour.
                    _logger.LogError(ex, "Error occurred while releasing expired cart items.");
                }
            }

            _logger.LogInformation("Cart expiry background service stopped.");
        }

        private async Task ReleaseExpiredItemsAsync()
        {
            // BackgroundService is a singleton; ApplicationDbContext is scoped.
            // We must create a new scope each time we need the DB.
            using var scope = _scopeFactory.CreateScope();
            var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();

            // Pass null = release expired items for ALL users.
            await cartService.ReleaseExpiredCartItemsAsync(userId: null);

            _logger.LogInformation(
                "Cart expiry check completed at {Time}.", DateTime.UtcNow);
        }
    }
}