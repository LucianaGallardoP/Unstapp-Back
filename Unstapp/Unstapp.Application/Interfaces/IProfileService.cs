using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface IProfileService
    {
        Task<ServiceResult<ProfileResponseDto>> GetProfileAsync(int requestedUserId, int tokenUserId);
        Task<ServiceResult<ProfileResponseDto>> UpdateProfileAsync(int currentUserId, UpdateProfileDto dto);
        Task<ServiceResult<FollowToggleResponseDto>> ToggleFollowAsync(int followerUserId, int followedUserId);
    }
}
