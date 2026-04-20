using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Testify.Interfaces;
using Testify.Settings;

namespace Testify.Services
{
    public class SmtpEmailService : IAppEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            using var message = CreateMessage(toEmail, subject, htmlBody);
            await SendAsync(message);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string fromEmail, string fromName)
        {
            using var message = CreateMessage(toEmail, subject, htmlBody, fromEmail, fromName);
            await SendAsync(message);
        }

        public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlBody, byte[] attachment, string attachmentName)
        {
            using var message = CreateMessage(toEmail, subject, htmlBody);
            using var stream = new MemoryStream(attachment);
            message.Attachments.Add(new Attachment(stream, attachmentName, "application/pdf"));
            await SendAsync(message);
        }

        public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlBody, byte[] attachment, string attachmentName, string fromEmail, string fromName)
        {
            using var message = CreateMessage(toEmail, subject, htmlBody, fromEmail, fromName);
            using var stream = new MemoryStream(attachment);
            message.Attachments.Add(new Attachment(stream, attachmentName, "application/pdf"));
            await SendAsync(message);
        }

        private MailMessage CreateMessage(string toEmail, string subject, string htmlBody, string? fromEmail = null, string? fromName = null)
        {
            var senderEmail = fromEmail ?? _settings.SenderEmail;
            var senderName = fromName ?? _settings.SenderName;

            var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);
            return message;
        }

        private async Task SendAsync(MailMessage message)
        {
            try
            {
                using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPass),
                    EnableSsl = _settings.EnableSsl
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent to {To}: {Subject}", message.To, message.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}: {Subject}", message.To, message.Subject);
                throw;
            }
        }
    }
}
