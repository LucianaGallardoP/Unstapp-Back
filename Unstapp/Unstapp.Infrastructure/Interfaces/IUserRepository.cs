using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities;
using Unstapp.Shared.DTOs;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByDniAsync(string Dni);
        Task AddAsync(User user);
        Task<List<int>> GetCareerIdsByUserIdAsync(int userId);
        Task<List<string>> GetRoleNameByUserIdAsync(int userId);
        Task<User?> GetByIdAsync(int userId);
        Task<List<User>> SearchUsersAsync(string term, int currentUserId);
        Task<User?> GetProfileByIdAsync(int userId);
        Task<ProfileMetricsDto> GetProfileMetricsAsync(int userId);
        Task<bool> ExistsAsync(int userId);
        Task UpdateAsync(User user);
        Task<User?> GetUserContextByIdAsync(int userId);
    }
}
