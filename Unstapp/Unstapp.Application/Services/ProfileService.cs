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
using Unstapp.Shared.Interfaces;

namespace Unstapp.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserFollowRepository _userFollowRepository;
        private readonly IMediaStorageService _mediaStorageService;

        public ProfileService(
            IUserRepository userRepository,
            IUserFollowRepository userFollowRepository,
            IMediaStorageService mediaStorageService)
        {
            _userRepository = userRepository;
            _userFollowRepository = userFollowRepository;
            _mediaStorageService = mediaStorageService;
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

        public async Task<ServiceResult<ProfileResponseDto>> UpdateProfileAsync(
            int currentUserId,
            UpdateProfileDto dto)
        {
            var user = await _userRepository.GetProfileByIdAsync(currentUserId);

            if (user == null)
                return ServiceResult<ProfileResponseDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "USER_NOT_FOUND",
                    "Usuario no encontrado."
                );

            if (dto.Bio != null)
            {
                user.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim();
            }
            else if (dto.RemoveBio)
            {
                user.Bio = null;
            }

            if (dto.AvatarFile != null)
            {
                var avatarUpload = await _mediaStorageService.UploadUserAvatarAsync(
                    dto.AvatarFile,
                    currentUserId
                );

                if (!avatarUpload.Success)
                    return ServiceResult<ProfileResponseDto>.Fail(
                        avatarUpload.Error!.StatusCode,
                        avatarUpload.Error.Code,
                        avatarUpload.Error.Message
                    );

                user.AvatarUrl = avatarUpload.Data;
            }
            else if (dto.RemoveAvatar)
            {
                user.AvatarUrl = null;
            }


            if (dto.CoverFile != null)
            {
                var coverUpload = await _mediaStorageService.UploadUserCoverAsync(
                    dto.CoverFile,
                    currentUserId
                );

                if (!coverUpload.Success)
                    return ServiceResult<ProfileResponseDto>.Fail(
                        coverUpload.Error!.StatusCode,
                        coverUpload.Error.Code,
                        coverUpload.Error.Message
                    );

                user.CoverUrl = coverUpload.Data;
            }
            else if (dto.RemoveCover)
            {
                user.CoverUrl = null;
            }

            await _userRepository.UpdateAsync(user);

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
                IsOwnProfile = true,
                IsFollowing = false
            };

            return ServiceResult<ProfileResponseDto>.Ok(response);
        }

    }
}
