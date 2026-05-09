using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Infrastructure.Entities.Enums;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface IPostService
    {
        Task<ServiceResult<PostDto>> CreateAsync(int userId, CreatePostDto dto);
        Task<ServiceResult<List<PostDto>>> GetAllAsync(int userId, PostFilter filter);
    }
}
