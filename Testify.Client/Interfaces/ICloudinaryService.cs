namespace Testify.Client.Interfaces
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadFileAsync(Stream fileStream, string fileName);
        Task<bool> DeleteFileAsync(string publicId);
    }
}
