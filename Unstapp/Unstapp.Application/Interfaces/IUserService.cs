using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult<UserContextDto>> GetUserContextAsync(int userId);
    }
}
