using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserFollowRepository _userFollowRepository;

        public ProfileService(
            IUserRepository userRepository,
            IUserFollowRepository userFollowRepository)
        {
            _userRepository = userRepository;
            _userFollowRepository = userFollowRepository;
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

            if (!isOwnProfile)
            {
                isFollowing = await _userFollowRepository.ExistsAsync(currentUserId, profileUserId);
            }

            var metrics = await _userRepository.GetProfileMetricsAsync(profileUserId);

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
                IsFollowing = isFollowing,
                PostsCount = metrics.PostsCount,
                FollowersCount = metrics.FollowersCount,
                FollowingCount = metrics.FollowingCount
            };

            return ServiceResult<ProfileResponseDto>.Ok(response);
        }
        public async Task<ServiceResult<FollowToggleResponseDto>> ToggleFollowAsync(
            int followerUserId,
            int followedUserId)
        {
            if (followerUserId == followedUserId)
                return ServiceResult<FollowToggleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "CANNOT_FOLLOW_YOURSELF",
                    "No puedes seguir a t propio perfil."
                    );

            var profileUser = await _userRepository.GetByIdAsync(followedUserId);

            if (profileUser == null)
                return ServiceResult<FollowToggleResponseDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "USER_NOT_FOUND",
                    "Usuario no encontrado.");

            var alreadyFollowing = await _userFollowRepository.ExistsAsync(followerUserId, followedUserId);

            if(alreadyFollowing)
            {
                await _userFollowRepository.DeleteAsync(followerUserId, followedUserId);

                await _userFollowRepository.SaveChangesAsync();

                return ServiceResult<FollowToggleResponseDto>.Ok(new FollowToggleResponseDto
                {
                    ProfileUserId = followedUserId,
                    IsFollowing = false,
                    Message = "Dejaste de seguir a este perfil."
                });
            }

            var follow = new UserFollow
            {
                FollowerUserId = followerUserId,
                FollowedUserId = followedUserId,
                FollowedAt = DateTime.UtcNow
            };

            await _userFollowRepository.AddAsync(follow);
            await _userFollowRepository.SaveChangesAsync();

            return ServiceResult<FollowToggleResponseDto>.Ok(new FollowToggleResponseDto
            {
                ProfileUserId = followedUserId,
                IsFollowing = true,
                Message = "Comenzaste a seguir a este perfil."
            });
        }

    }
}
