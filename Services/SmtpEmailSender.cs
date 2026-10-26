using ClinicManagementSystem.API.Interfaces;
using ClinicManagementSystem.API.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ClinicManagementSystem.API.Services;

public class SmtpEmailSender(IOptions<EmailSettings> options) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        var s = options.Value;

        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(s.FromName, s.FromAddress));
        mime.To.Add(new MailboxAddress(message.ToName, message.To));
        mime.Subject = message.Subject;
        mime.Body = new TextPart("plain") { Text = message.Body };

        using var client = new SmtpClient();
        // Auto picks the right encryption from the port (465 = SSL, 587 = STARTTLS)
        await client.ConnectAsync(s.Host, s.Port, SecureSocketOptions.Auto, ct);

        if (!string.IsNullOrEmpty(s.Username))
            await client.AuthenticateAsync(s.Username, s.Password, ct);

        await client.SendAsync(mime, ct);
        await client.DisconnectAsync(quit: true, ct);
    }
}