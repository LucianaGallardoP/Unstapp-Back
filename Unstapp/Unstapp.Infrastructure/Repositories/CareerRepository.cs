using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Unstapp.Infrastructure.Data;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;

namespace Unstapp.Infrastructure.Repositories
{
    public class CareerRepository : ICareerRepository
    {
        private readonly AppDbContext _context;
        public CareerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CareerExistsAsync(int careerId)
        {
            return await _context.Careers.AnyAsync(c => c.CareerId == careerId);
        }
        public async Task<List<Career>> GetAllCareersAsync()
        {
            return await _context.Careers
                .AsNoTracking()
                .Include(c => c.Faculty)
                .Include(c => c.UserCareers)
                    .ThenInclude(uc => uc.User)
                .ToListAsync();
        }
        public async Task<UserCareer?> GetUserCareerAsync(int userId)
        {
            return await _context.UserCareers
                .AsNoTracking()
                .Include(uc => uc.Career)
                    .ThenInclude(c => c.Faculty)
                .FirstOrDefaultAsync(uc => uc.UserId == userId);
        }

        public async Task<List<int>> GetExistingCareerIdsAsync(List<int> careerIds)
        {
            return await _context.Careers
                .Where(c => careerIds.Contains(c.CareerId))
                .Select(c => c.CareerId)
                .ToListAsync();
        }
    }
}
