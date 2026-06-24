using System.Collections.Generic;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface ICareerAdminService
    {
        Task<ServiceResult<List<CareerAdminResponseDto>> GetAllCareersAsync();
    }
}