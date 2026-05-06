using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Unstapp.Application.Interfaces;

namespace Unstapp.Infrastructure.Services
{
    public class CloudinaryMediaStorageService : IMediaStorageService
    {
        private readonly Cloudinary _cloudinary;

        private const long MaxImageSize = 5 * 1024 * 1024; // 5 MB
        private const long MaxVideoSize = 20 * 1024 * 1024; // 20 MB

        private static readonly string[] _AllowedImageTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private static readonly string[] _AllowedVideoTypes =
        {
            "video/mp4",
            "video/webm",
            "video/quicktime"
        };

        public CloudinaryMediaStorageService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string?> UploadPostMediaAsync(IFormFile file, int userId)
        {
            if (file == null || file.Length == 0)
                return null;

            var contentType = file.ContentType.ToLower();

            var isImage = _AllowedImageTypes.Contains(contentType);
            var isVideo = _AllowedVideoTypes.Contains(contentType);

            if(!isImage && !isVideo)
                throw new ArgumentException("Solo se permiten imagenes JPG, PNG, WEBP o videos MP4, WEBM, MOV.");

            if (isImage && file.Length > MaxImageSize)
                throw new ArgumentException("La imagen no puede superar los 5 MB.");

            if (isVideo && file.Length > MaxVideoSize)
                throw new ArgumentException("El video no puede superar los 20 MB.");

            var uniqueId = Guid.NewGuid().ToString("N");

            await using var stream = file.OpenReadStream();

            if (isImage)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription($"post_{uniqueId}", stream),
                    Folder = "unstapp/posts/images",
                    PublicId = $"post_user_{userId}_{uniqueId}",
                    Overwrite = false,

                    Transformation = new Transformation()
                        .Quality("auto")
                        .FetchFormat("auto")
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                    throw new Exception(result.Error.Message);

                return result.SecureUrl?.ToString();
            }

            if (isVideo)
            {
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription($"post_{uniqueId}", stream),
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

            if (!_AllowedImageTypes.Contains(contentType))
                throw new ArgumentException("El avatar debe ser una imagen JPG, PNG o WEBP.");

            if (file.Length > MaxImageSize)
                throw new ArgumentException("El avatar no puede superar los 5 MB.");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription($"avatar_user_{userId}", stream),
                Folder = "unstapp/users/avatars",
                PublicId = $"user_{userId}_avatar",
                Overwrite = true,

                Transformation = new Transformation()
                    .Width(500)
                    .Height(500)
                    .Crop("fill")
                    .Gravity("face")
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception(result.Error.Message);

            return result.SecureUrl?.ToString();
        }
    }
}
