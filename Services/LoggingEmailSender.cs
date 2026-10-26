using ClinicManagementSystem.API.Interfaces;

namespace ClinicManagementSystem.API.Services;

public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        logger.LogInformation("[FAKE EMAIL] To: {To} | Subject: {Subject}{NewLine}{Body}",
            message.To, message.Subject, Environment.NewLine, message.Body);
        return Task.CompletedTask;
    }
}