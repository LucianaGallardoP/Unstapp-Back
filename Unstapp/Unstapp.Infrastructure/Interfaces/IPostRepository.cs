using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface IPostRepository
    {
        Task AddAsync(Post post);
        Task<Post?> GetByIdWithRelationsAsync(int postId);
        Task<List<Post>> GetAllWithRelationsAsync();
    }
}
