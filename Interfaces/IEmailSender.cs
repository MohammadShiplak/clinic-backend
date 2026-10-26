namespace ClinicManagementSystem.API.Interfaces;

public record EmailMessage(string To, string ToName, string Subject, string Body);

public interface IEmailSender
{
    /// Throws if delivery fails. The caller decides about retries.
    Task SendAsync(EmailMessage message, CancellationToken ct);
}