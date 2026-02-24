using Microsoft.AspNetCore.Http;

namespace Testify.Interfaces
{
    /// <summary>
    /// Abstraction for file storage operations (upload, delete, URL generation).
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves an uploaded file to storage and returns the relative URL.
        /// </summary>
        /// <param name="file">The uploaded file.</param>
        /// <param name="subFolder">Optional sub-folder within the upload directory.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Relative URL to the stored file.</returns>
        Task<string> SaveFileAsync(IFormFile file, string? subFolder = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file from storage by its relative URL.
        /// </summary>
        /// <param name="relativeUrl">The relative URL returned by SaveFileAsync.</param>
        /// <returns>True if the file was deleted; false if it was not found.</returns>
        bool DeleteFile(string relativeUrl);

        /// <summary>
        /// Gets the absolute file system path for a relative URL.
        /// </summary>
        string GetAbsolutePath(string relativeUrl);
    }
}
