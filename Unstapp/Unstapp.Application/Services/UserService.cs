using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
    }
}
