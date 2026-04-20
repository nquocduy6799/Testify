namespace Testify.Settings
{
    public class EmailSettings
    {
        public const string SectionName = "Email";

        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPass { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = "Testify";
        public bool EnableSsl { get; set; } = true;
    }
}
