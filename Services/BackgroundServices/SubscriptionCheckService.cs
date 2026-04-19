using PayroTech.Services.BackgroundServices;

namespace PayroTech.Services.BackgroundServices;

/// <summary>
/// Background service that runs daily to check subscription status and send warnings.
/// Runs at 9:00 AM every day to check for expiring subscriptions.
/// </summary>
public class SubscriptionCheckService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionCheckService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Run once per day
    private readonly TimeSpan _targetTime = new TimeSpan(9, 0, 0); // 9:00 AM

    public SubscriptionCheckService(
        IServiceProvider serviceProvider,
        ILogger<SubscriptionCheckService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscription Check Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var nextRun = CalculateNextRunTime(now);
                var delay = nextRun - now;

                _logger.LogInformation("Next subscription check scheduled for: {NextRun}", nextRun);

                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await CheckSubscriptionsAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Subscription Check Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Subscription Check Service");
                // Wait 1 hour before retrying on error
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private DateTime CalculateNextRunTime(DateTime now)
    {
        var today = now.Date;
        var targetToday = today.Add(_targetTime);

        // If target time today has passed, schedule for tomorrow
        if (now >= targetToday)
        {
            return today.AddDays(1).Add(_targetTime);
        }

        return targetToday;
    }

    private async Task CheckSubscriptionsAsync()
    {
        _logger.LogInformation("Starting subscription check at {Time}", DateTime.Now);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var subscriptionMonitor = scope.ServiceProvider.GetRequiredService<ISubscriptionMonitorService>();

            await subscriptionMonitor.CheckAndSendSubscriptionWarningsAsync();

            _logger.LogInformation("Subscription check completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during subscription check");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Subscription Check Service is stopping");
        await base.StopAsync(stoppingToken);
    }
}
