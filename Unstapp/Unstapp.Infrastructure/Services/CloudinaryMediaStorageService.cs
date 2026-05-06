using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Unstapp.Application.Interfaces;

namespace Unstapp.Infrastructure.Services
{
    public class CloudinaryMediaStorageService : IMediaStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryMediaStorageService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string?> UploadPostMediaAsync(IFormFile file, int userId)
        {
            if (file == null || file.Length == 0)
                return null;

            var contentType = file.ContentType.ToLower();

            var uniqueId = Guid.NewGuid().ToString("N");

            await using var stream = file.OpenReadStream();

            if (contentType.StartsWith("image/"))
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "unstapp/posts/images",
                    PublicId = $"post_user_{userId}_{uniqueId}",
                    Overwrite = false
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                    throw new Exception(result.Error.Message);

                return result.SecureUrl?.ToString();
            }

            if (contentType.StartsWith("video/"))
            {
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "unstapp/posts/videos",
                    PublicId = $"post_user_{userId}_{uniqueId}",
                    Overwrite = false
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                    throw new Exception(result.Error.Message);

                return result.SecureUrl?.ToString();
            }

            throw new ArgumentException("Solo se permiten imagenes o videos");
        }

        public async Task<string?> UploadUserAvatarAsync(IFormFile file, int userId)
        {
            if (file == null || file.Length == 0)
                return null;

            var contentType = file.ContentType.ToLower();

            if (!contentType.StartsWith("image/"))
                throw new Exception("El avatar debe ser una imagen");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "unstapp/users/avatars",
                PublicId = $"user_{userId}_avatar",
                Overwrite = true
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception(result.Error.Message);

            return result.SecureUrl?.ToString();
        }
    }
}
