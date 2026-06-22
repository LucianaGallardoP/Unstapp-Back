using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Data;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs;
using Unstapp.Shared.Helpers;

namespace Unstapp.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByDniAsync(string Dni)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.DNI == Dni);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<int>> GetCareerIdsByUserIdAsync(int userId)
        {
            return await _context.UserCareers
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CareerId)
                .ToListAsync();
        }

        public async Task<List<string>> GetRoleNameByUserIdAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<List<User>> SearchUsersAsync(string term, int currentUserId)
        {
            term = SearchHelpers.RemoveDiacritics(term.Trim());

            return await _context.Users
                .Where(u =>
                    u.UserId != currentUserId &&
                    (
                        EF.Functions.ILike(PostgresDbFunctions.Unaccent(u.Name), $"%{term}%") ||
                        EF.Functions.ILike(PostgresDbFunctions.Unaccent(u.LastName), $"%{term}%") ||
                        EF.Functions.ILike(PostgresDbFunctions.Unaccent(u.Name + " " + u.LastName), $"%{term}%")
                    ))
                .OrderBy(u => u.Name)
                .ThenBy(u => u.LastName)
                .Take(20)
                .ToListAsync();
        }

        public async Task<User?> GetProfileByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserCareers)
                    .ThenInclude(uc => uc.Career)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<ProfileMetricsDto> GetProfileMetricsAsync(int userId)
        {
            return await _context.Users
                .Where (u => u.UserId == userId)
                .Select(u => new ProfileMetricsDto
                {
                    PostsCount = _context.Posts
                        .Count(p => p.UserId == userId),
                    FollowersCount = _context.UserFollow
                        .Count(uf => uf.FollowedUserId == userId),
                    FollowingCount = _context.UserFollow
                        .Count(uf => uf.FollowerUserId == userId)
                })
                .FirstOrDefaultAsync() ?? new ProfileMetricsDto();
        }

        public async Task<bool> ExistsAsync(int userId)
        {
            return await _context.Users
                .AnyAsync(u => u.UserId == userId);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserContextByIdAsync(int userId)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.UserCareers)
                    .ThenInclude(uc => uc.Career)
                        .ThenInclude(c => c.Faculty)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }
}
