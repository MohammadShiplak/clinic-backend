using ClinicManagementSystem.API.Interfaces;
using ClinicManagementSystem.API.Options;
using Microsoft.Extensions.Options;

namespace ClinicManagementSystem.API.Workers;

public class AppointmentReminderWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<ReminderSettings> options,
    ILogger<AppointmentReminderWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(options.Value.CheckIntervalMinutes));

        // do/while: run once IMMEDIATELY at startup (catch-up after downtime), then every interval.
        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();   // fresh DbContext per tick
                var reminders = scope.ServiceProvider.GetRequiredService<IAppointmentReminderService>();

                var processed = await reminders.SendDueRemindersAsync(stoppingToken);
                if (processed > 0)
                    logger.LogInformation("Created reminders for {Count} appointment(s).", processed);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A failed tick must NEVER kill the worker.
                logger.LogError(ex, "Reminder run failed; will retry on the next tick.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}