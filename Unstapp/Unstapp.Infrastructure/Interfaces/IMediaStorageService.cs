using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.Interfaces
{
    public interface IMediaStorageService
    {
        Task<string?> UploadPostMediaAsync(IFormFile file, int userId);
        Task<string?> UploadUserAvatarAsync(IFormFile file, int userId);
    }
}
