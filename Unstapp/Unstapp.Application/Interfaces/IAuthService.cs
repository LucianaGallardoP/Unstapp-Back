using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
        Task RegisterAsync(RegisterRequestDto dto);
        Task<ServiceResult<MessageResponseDto>> VerifyFirstTimeAsync(VerifyFirstTimeRequestDto dto);
        Task<ServiceResult<MessageResponseDto>> SetInitialPasswordAsync(SetInitialPasswordRequestDto dto);
    }
}
