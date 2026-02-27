namespace Testify.Settings
{
    /// <summary>
    /// Configuration settings for file upload functionality.
    /// </summary>
    public class FileUploadSettings
    {
        public const string SectionName = "FileUpload";

        /// <summary>Maximum allowed file size in bytes (default: 25 MB).</summary>
        public long MaxFileSizeBytes { get; set; } = 25 * 1024 * 1024;

        /// <summary>Root directory for uploaded files (relative to wwwroot).</summary>
        public string UploadDirectory { get; set; } = "uploads/chat";

        /// <summary>Allowed file extensions (lowercase, with leading dot).</summary>
        public string[] AllowedExtensions { get; set; } =
        {
            // Images
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg",
            // Documents
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".txt", ".csv", ".rtf", ".odt", ".ods", ".odp",
            // Archives
            ".zip", ".rar", ".7z", ".tar", ".gz",
            // Code / Data
            ".json", ".xml", ".html", ".css", ".js", ".ts", ".cs", ".sql",
            // Media
            ".mp4", ".mp3", ".wav", ".avi", ".mov", ".mkv"
        };

        /// <summary>MIME types considered as images (for thumbnail / inline preview).</summary>
        public static readonly HashSet<string> ImageMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp", "image/svg+xml"
        };

        /// <summary>Check whether the given MIME type represents an image.</summary>
        public static bool IsImageMimeType(string? mimeType)
        {
            return !string.IsNullOrWhiteSpace(mimeType) && ImageMimeTypes.Contains(mimeType);
        }
    }
}
