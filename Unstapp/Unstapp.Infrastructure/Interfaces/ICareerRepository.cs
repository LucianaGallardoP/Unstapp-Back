using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface ICareerRepository
    {
        Task<bool> CareerExistsAsync(int careerId);
        Task<List<Career>> GetAllCareersAsync();
        Task<UserCareer?> GetUserCareerAsync(int userId);
        Task<List<int>> GetExistingCareerIdsAsync(List<int> careerIds);
    }
}
