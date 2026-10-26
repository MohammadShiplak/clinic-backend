using ClinicManagementSystem.API.Interfaces;
using ClinicManagementSystem.API.Options;
using Microsoft.Extensions.Options;

namespace ClinicManagementSystem.API.Workers;

public class NotificationEmailWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<EmailSettings> options,
    ILogger<NotificationEmailWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.DispatchIntervalSeconds));

        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationEmailDispatcher>();

                var sent = await dispatcher.DispatchPendingAsync(stoppingToken);
                if (sent > 0)
                    logger.LogInformation("Sent {Count} notification email(s).", sent);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Email dispatch run failed; will retry on the next tick.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}