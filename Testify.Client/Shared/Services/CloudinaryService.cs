using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Components.Forms;
using System.Security.Principal;
using Testify.Client.Interfaces;

namespace Testify.Client.Shared.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<(string Url, string PublicId)> UploadFileAsync(Stream fileStream, string fileName)
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = "testify/attachments",
                UseFilename = true,
                UniqueFilename = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Upload failed: {uploadResult.Error.Message}");
            }

            return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
        }

        public async Task<bool> DeleteFileAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId) { ResourceType = ResourceType.Raw };
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
    }
}
