using Unstapp.Application.DTOs;
using Unstapp.Application.DTOs.Auth;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<LoginResponseDto?>> LoginAsync(LoginRequestDto dto);
        Task RegisterAsync(RegisterRequestDto dto);
        Task<ServiceResult<MessageResponseDto>> VerifyFirstTimeAsync(VerifyFirstTimeRequestDto dto);
        Task<ServiceResult<MessageResponseDto>> SetInitialPasswordAsync(SetInitialPasswordRequestDto dto);
        Task<ServiceResult<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto dto);
        Task<ServiceResult<bool>> ResetPasswordAsync(ResetPasswordRequestDto dto);
    }
}
