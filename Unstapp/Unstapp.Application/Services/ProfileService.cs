using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;

        public ProfileService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ServiceResult<ProfileResponseDto>> GetProfileAsync(
            int profileUserId,
            int currentUserId)
        {            
            var user = await _userRepository.GetProfileByIdAsync(profileUserId);

            if (user == null)
                return ServiceResult<ProfileResponseDto>.Fail(
                        StatusCodes.Status404NotFound,
                        "USER_NOT_FOUND",
                        "Usuario no encontrado."
                    );

            var isOwnProfile = profileUserId == currentUserId;

            var isFollowing = false;

            // TODO: VERIFICACION DE FOLLOWING

            var response = new ProfileResponseDto
            {
                UserId = user.UserId,
                FullName = $"{user.Name} {user.LastName}".Trim(),
                Careers = user.UserCareers
                    .Select(uc => uc.Career.Name)
                    .ToList(),
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                CoverUrl = user.CoverUrl,
                IsOwnProfile = isOwnProfile,
                IsFollowing = isFollowing
            };

            return ServiceResult<ProfileResponseDto>.Ok(response);
        }
    }
}
