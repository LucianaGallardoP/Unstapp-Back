using Microsoft.AspNetCore.Http;
using Unstapp.Application.DTOs;
using Unstapp.Application.DTOs.Users;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICareerRepository _careerRepository;

        public UserService(
            IUserRepository userRepository,
            ICareerRepository careerRepository)
        {
            _userRepository = userRepository;
            _careerRepository = careerRepository;
        }

        public async Task<ServiceResult<UserContextDto>> GetUserContextAsync(int userId)
        {
            var user = await _userRepository.GetUserContextByIdAsync(userId);

            if (user == null)
                return ServiceResult<UserContextDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "USER_NOT_FOUND",
                    "Usuario no encontrado."
                );

            var response = new UserContextDto
            {
                UserId = userId,
                FullName = $"{user.Name} {user.LastName}".Trim(),
                CareerDtos = user.UserCareers
                    .Select(uc => new UserContextCareerDto
                    {
                        CareerId = uc.CareerId,
                        CareerName = uc.Career.Name,
                        FacultyName = uc.Career.Faculty.Name
                    })
                    .ToList()
            };

            return ServiceResult<UserContextDto>.Ok(response);
        }

        public async Task<ServiceResult<bool>> UpdateWhatsAppNotificationsAsync(int userId, bool enable)
        {
            if(userId <= 0)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_USER_ID",
                    "El ID del usuario no es válido."
                );

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "USER_NOT_FOUND",
                    "Usuario no encontrado."
                );

            user.WhatsAppNotificationsEnabled = enable;

            await _userRepository.UpdateAsync(user);

            return ServiceResult<bool>.Ok(user.WhatsAppNotificationsEnabled);
        }

        public async Task<ServiceResult<UserCareerResponseDto?>> GetMyCareerAsync(int userId)
        {
            if(userId <= 0)
                return ServiceResult<UserCareerResponseDto?>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_USER_ID",
                    "El ID del usuario no es válido."
                );

            var roles = await _userRepository.GetRoleNameByUserIdAsync(userId);

            var canAccessCareer = roles.Any(r =>
                r.Equals("Alumno", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("Docente", StringComparison.OrdinalIgnoreCase)
            );

            if (!canAccessCareer)
                return ServiceResult<UserCareerResponseDto?>.Ok(null);

            var userCareer = await _careerRepository.GetUserCareerAsync(userId);

            if(userCareer == null)
                return ServiceResult<UserCareerResponseDto?>.Fail(
                    StatusCodes.Status404NotFound,
                    "CAREER_NOT_ASSIGNED",
                    "El usuario no tiene una carrera asignada."
                );

            if(userCareer.Career == null)
                return ServiceResult<UserCareerResponseDto?>.Fail(
                    StatusCodes.Status404NotFound,
                    "CAREER_NOT_FOUND",
                    "La carrera asignada al usuario no fue encontrada."
                );

            var response = new UserCareerResponseDto
            {
                CareerName = userCareer.Career.Name
            };

            return ServiceResult<UserCareerResponseDto?>.Ok(response);
        }
    }
}
