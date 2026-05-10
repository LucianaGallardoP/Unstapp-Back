using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByDniAsync(string Dni);
        Task AddAsync(User user);
        Task<List<int>> GetCareerIdsByUserIdAsync(int userId);
        Task<List<string>> GetRoleNameByUserIdAsync(int userId);
        Task<User?> GetByIdAsync(int userId);
    }
}
