using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Shared.Interfaces
{
    public interface IMediaStorageService
    {
        Task<ServiceResult<string?>> UploadPostMediaAsync(IFormFile file, int userId);
        Task<ServiceResult<string?>> UploadUserAvatarAsync(IFormFile file, int userId);
    }
}
