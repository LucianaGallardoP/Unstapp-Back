using System.Collections.Generic;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;

namespace Unstapp.Application.Interfaces
{
    public interface ICareerAdminService
    {
        Task<IEnumerable<CareerAdminResponseDto>> GetAllCareersAsync();
    }
}