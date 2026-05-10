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
    }
}
