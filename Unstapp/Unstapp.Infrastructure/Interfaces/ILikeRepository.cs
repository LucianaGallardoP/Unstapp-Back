using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface ILikeRepository
    {
        Task<Like?> GetByPostAndUserAsync(int postId, int userId);
        Task<bool> AddAsync(Like like);
        Task RemoveAsync(Like like);
    }
}
