using Microsoft.AspNetCore.Identity;
using Testify.Data;
using Testify.Interfaces;

namespace Testify.Components.Account
{
    internal sealed class IdentitySmtpEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IAppEmailService _emailService;

        public IdentitySmtpEmailSender(IAppEmailService emailService)
        {
            _emailService = emailService;
        }

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
            _emailService.SendEmailAsync(email, "Confirm your email",
                $"<h2>Welcome to Testify!</h2><p>Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.</p>");

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
            _emailService.SendEmailAsync(email, "Reset your password",
                $"<h2>Password Reset</h2><p>Please reset your password by <a href='{resetLink}'>clicking here</a>.</p>");

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
            _emailService.SendEmailAsync(email, "Reset your password",
                $"<h2>Password Reset</h2><p>Your password reset code is: <strong>{resetCode}</strong></p>");
    }
}
