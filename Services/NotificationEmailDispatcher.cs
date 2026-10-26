using ClinicManagementSystem.API.Entities;
using ClinicManagementSystem.API.Interfaces;
using ClinicManagementSystem.API.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicManagementSystem.API.Services;

public class NotificationEmailDispatcher(
    ClinicDbContext context,
    IEmailSender emailSender,
    IOptions<EmailSettings> options,
    TimeProvider clock,
    ILogger<NotificationEmailDispatcher> logger) : INotificationEmailDispatcher
{
    private const int BatchSize = 50;

    public async Task<int> DispatchPendingAsync(CancellationToken ct)
    {
        var maxAttempts = options.Value.MaxAttempts;

        // Tracked on purpose: we update these rows below. Oldest first = fair.
        var pending = await context.Notifications
            .Include(n => n.Person)
            .Where(n => n.EmailStatus == EmailDeliveryStatus.Pending)
            .OrderBy(n => n.NotificationId)
            .Take(BatchSize)
            .ToListAsync(ct);

        var sent = 0;

        foreach (var n in pending)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(n.Person.Email))
            {
                n.EmailStatus = EmailDeliveryStatus.None;   // nobody to email: leave the tray
            }
            else
            {
                try
                {
                    await emailSender.SendAsync(
                        new EmailMessage(n.Person.Email, n.Person.Name, n.Title, BuildBody(n)), ct);

                    n.EmailStatus = EmailDeliveryStatus.Sent;
                    n.EmailSentAtUtc = clock.GetUtcNow().UtcDateTime;
                    sent++;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    n.EmailAttempts++;
                    n.EmailLastError = Truncate(ex.Message, 500);

                    if (n.EmailAttempts >= maxAttempts)
                        n.EmailStatus = EmailDeliveryStatus.Failed;   // stop retrying (poison-pill guard)

                    logger.LogWarning(ex, "Email for notification {Id} failed (attempt {Attempt}/{Max}).",
                        n.NotificationId, n.EmailAttempts, maxAttempts);
                }
            }

            // Save after EACH email: a crash can lose at most the one in flight,
            // instead of re-sending the whole batch.
            await context.SaveChangesAsync(ct);
        }

        return sent;
    }

    private static string BuildBody(Notification n) =>
        $"{n.Message}{Environment.NewLine}{Environment.NewLine}" +
        "Clinic Management System" + Environment.NewLine +
        "This is an automated message. Please do not reply.";

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
}