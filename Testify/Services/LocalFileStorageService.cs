using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Testify.Interfaces;
using Testify.Settings;

namespace Testify.Services
{
    /// <summary>
    /// Local file system implementation of <see cref="IFileStorageService"/>.
    /// Files are stored under wwwroot/{UploadDirectory}.
    /// </summary>
    public sealed class LocalFileStorageService : IFileStorageService
    {
        private readonly FileUploadSettings _settings;
        private readonly string _wwwRootPath;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(
            IOptions<FileUploadSettings> settings,
            IWebHostEnvironment environment,
            ILogger<LocalFileStorageService> logger)
        {
            _settings = settings.Value;
            _wwwRootPath = environment.WebRootPath;
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string? subFolder = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(file);

            // Build target directory
            var relativeFolderParts = new List<string> { _settings.UploadDirectory };
            if (!string.IsNullOrWhiteSpace(subFolder))
            {
                relativeFolderParts.Add(subFolder);
            }

            // Append year/month for automatic partitioning
            var now = DateTime.UtcNow;
            relativeFolderParts.Add(now.ToString("yyyy"));
            relativeFolderParts.Add(now.ToString("MM"));

            var relativeFolder = Path.Combine(relativeFolderParts.ToArray());
            var absoluteFolder = Path.Combine(_wwwRootPath, relativeFolder);

            Directory.CreateDirectory(absoluteFolder);

            // Generate unique file name to prevent collisions
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueName = $"{Guid.NewGuid():N}{extension}";

            var absolutePath = Path.Combine(absoluteFolder, uniqueName);
            var relativeUrl = $"/{relativeFolder.Replace('\\', '/')}/{uniqueName}";

            try
            {
                await using var stream = new FileStream(absolutePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
                await file.CopyToAsync(stream, cancellationToken);

                _logger.LogInformation("File saved: {FileName} -> {RelativeUrl} ({Size} bytes)", file.FileName, relativeUrl, file.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save file: {FileName}", file.FileName);

                // Clean up partial file
                if (File.Exists(absolutePath))
                {
                    try { File.Delete(absolutePath); } catch { /* best effort */ }
                }

                throw;
            }

            return relativeUrl;
        }

        public bool DeleteFile(string relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl))
                return false;

            var absolutePath = GetAbsolutePath(relativeUrl);

            if (!File.Exists(absolutePath))
                return false;

            try
            {
                File.Delete(absolutePath);
                _logger.LogInformation("File deleted: {RelativeUrl}", relativeUrl);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file: {RelativeUrl}", relativeUrl);
                return false;
            }
        }

        public string GetAbsolutePath(string relativeUrl)
        {
            // Strip leading slash and normalize
            var normalized = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var absolutePath = Path.GetFullPath(Path.Combine(_wwwRootPath, normalized));

            // Prevent path traversal attacks
            if (!absolutePath.StartsWith(_wwwRootPath, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Access to path outside wwwroot is denied.");

            return absolutePath;
        }
    }
}
