using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;

namespace Unstapp.Application.Interfaces
{
    public interface IPostService
    {
        Task<PostDto> CreateAsync(CreatePostDto dto);
        Task<List<PostDto>> GetAllAsync();
    }
}
