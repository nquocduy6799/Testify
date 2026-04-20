namespace Testify.Interfaces
{
    public interface IAppEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendEmailAsync(string toEmail, string subject, string htmlBody, string fromEmail, string fromName);
        Task SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlBody, byte[] attachment, string attachmentName);
        Task SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlBody, byte[] attachment, string attachmentName, string fromEmail, string fromName);
    }
}
