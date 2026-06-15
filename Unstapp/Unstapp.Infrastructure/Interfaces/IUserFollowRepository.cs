using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface IUserFollowRepository
    {
        Task<bool> ExistsAsync(int followerUserId, int followedUserId);
        Task AddAsync(UserFollow follow);
        Task DeleteAsync(int followerUserId, int followedUserId);
        Task SaveChangesAsync();
    }
}
