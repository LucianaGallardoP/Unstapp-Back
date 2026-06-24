using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Data;

namespace Unstapp.Application.Services
{
    public class CareerAdminService : ICareerAdminService
    {
        private readonly AppDbContext _context;

        public CareerAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CareerAdminResponseDto>> GetAllCareersAsync()
        {
            return await _context.Careers
                .Include(c => c.Faculty)
                .Select(c => new CareerAdminResponseDto
                {
                    CareerId = c.CareerId,
                    Name = c.Name,
                    FacultyId = c.FacultyId,
                    FacultyName = c.Faculty != null ? c.Faculty.Name : "Sin Facultad"
                })
                .ToListAsync();
        }
    }
}