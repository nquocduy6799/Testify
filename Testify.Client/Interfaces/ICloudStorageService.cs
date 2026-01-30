using Microsoft.AspNetCore.Components.Forms;

namespace Testify.Client.Interfaces
{
    public interface ICloudStorageService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(IBrowserFile file);
        Task<bool> DeleteImageAsync(string publicId);
    }
}
