using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Pixora.BusinessLayer.Clients.Interfaces;
using Pixora.BusinessLayer.Settings;
using Pixora.Shared.Models;
using TinyHelpers.Extensions;

namespace Pixora.BusinessLayer.Clients;

public class EmailClient(IConfiguration configuration) : IEmailClient
{
    private readonly EmailSettings settings = configuration.GetSection(nameof(EmailSettings)).Get<EmailSettings>() ?? new EmailSettings();

    public async Task SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient();
        var message = CreateMessage(emailMessage);

        if (settings.IgnoreServerCertificateErrors)
        {
            client.ServerCertificateValidationCallback = (_, _, _, _) => true;
        }

        await client.ConnectAsync(settings.Host, settings.Port, settings.UseSsl, cancellationToken).ConfigureAwait(false);

        if (settings.UserName.HasValue() && settings.Password.HasValue())
        {
            await client.AuthenticateAsync(settings.UserName, settings.Password, cancellationToken).ConfigureAwait(false);
        }

        await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
    }

    private static MimeMessage CreateMessage(EmailMessage emailMessage)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(emailMessage.SenderName ?? emailMessage.SenderEmail, emailMessage.SenderEmail));
        message.To.AddRange(emailMessage.To?.Select(a => new MailboxAddress(a, a)) ?? []);
        message.Cc.AddRange(emailMessage.Cc?.Select(a => new MailboxAddress(a, a)) ?? []);
        message.Bcc.AddRange(emailMessage.Bcc?.Select(a => new MailboxAddress(a, a)) ?? []);
        message.ReplyTo.AddRange(emailMessage.ReplyTo?.Select(a => new MailboxAddress(a, a)) ?? []);

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = emailMessage.HtmlContent,
            TextBody = emailMessage.TextContent
        };

        message.Body = bodyBuilder.ToMessageBody();
        return message;
    }
}