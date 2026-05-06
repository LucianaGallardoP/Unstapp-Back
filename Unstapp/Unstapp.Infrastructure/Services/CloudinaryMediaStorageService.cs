using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Unstapp.Shared.DTOs.Common;
using Unstapp.Shared.Interfaces;

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

        public async Task<ServiceResult<string?>> UploadPostMediaAsync(IFormFile file, int userId)
        {
            if (file == null || file.Length == 0)
                return ServiceResult<string?>.Ok(null);

            var contentType = file.ContentType.ToLower();

            var isImage = _AllowedImageTypes.Contains(contentType);
            var isVideo = _AllowedVideoTypes.Contains(contentType);

            if(!isImage && !isVideo)
                return ServiceResult<string?>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_FILE_TYPE",
                    "Solo se permiten imagenes JPG, PNG, WEBP o videos MP4, WEBM, MOV."
                    );

            if (isImage && file.Length > MaxImageSize)
                return ServiceResult<string?>.Fail(
                    StatusCodes.Status400BadRequest,
                    "FILE_TOO_LARGE",
                    "La imagen no puede superar los 5 MB."
                    );

            if (isVideo && file.Length > MaxVideoSize)
                return ServiceResult<string?>.Fail(
                    StatusCodes.Status400BadRequest,
                    "FILE_TOO_LARGE",
                    "El video no puede superar los 20 MB."
                    );

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
                    return ServiceResult<string?>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "MEDIA_UPLOAD_FAILED",
                        result.Error.Message
                        );

                return ServiceResult<string?>.Ok(result.SecureUrl?.ToString());
            }
            else
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
                    return ServiceResult<string?>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "MEDIA_UPLOAD_FAILED",
                        result.Error.Message
                        );

                return ServiceResult<string?>.Ok(result.SecureUrl?.ToString());
            }
        }

        public async Task<ServiceResult<string?>> UploadUserAvatarAsync(IFormFile file, int userId)
        {
            if (file == null || file.Length == 0)
                return ServiceResult<string?>.Fail(
                    StatusCodes.Status400BadRequest,
                    "FILE_NULL",
                    "Debe subir un archivo."
                    );

            var contentType = file.ContentType.ToLower();

            if (!_AllowedImageTypes.Contains(contentType))
                return ServiceResult<string?>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_FILE_TYPE",
                    "La foto de perfil debe ser una imagen JPG, PNG o WEBP."
                    );

            if (file.Length > MaxImageSize)
                return ServiceResult<string?>.Fail(
                    StatusCodes.Status400BadRequest,
                    "FILE_TOO_LARGE",
                    "La foto de perfil no puede superar los 5 MB."
                    );

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
                return ServiceResult<string?>.Fail(
                    StatusCodes.Status500InternalServerError,
                    "MEDIA_UPLOAD_FAILED",
                    result.Error.Message
                    );

            return ServiceResult<string?>.Ok(result.SecureUrl?.ToString());
        }
    }
}
