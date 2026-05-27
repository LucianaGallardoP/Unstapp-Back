using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Data;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;

namespace Unstapp.Infrastructure.Repositories
{
    public class UserFollowRepository : IUserFollowRepository
    {
        private readonly AppDbContext _context;

        public UserFollowRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int followerUserId, int followedUserId)
        {
            return await _context.UserFollow.AnyAsync(uf =>
                uf.FollowerUserId == followerUserId &&
                uf.FollowedUserId == followedUserId);
        }

        public async Task AddAsync(UserFollow follow)
        {
            await _context.UserFollow.AddAsync(follow);
        }

        public async Task DeleteAsync(int followerUserId, int followedUserId)
        {
            var follow = await _context.UserFollow.FirstOrDefaultAsync(uf =>
                uf.FollowerUserId == followerUserId &&
                uf.FollowedUserId == followedUserId);

            if (follow != null)
                _context.UserFollow.Remove(follow);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
