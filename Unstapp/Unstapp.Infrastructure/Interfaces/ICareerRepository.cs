using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface ICareerRepository
    {
        Task<bool> CareerExistsAsync(int careerId);
        Task<UserCareer> GetUserCareerAsync(int userId);
    }
}
