using Microsoft.AspNetCore.Http;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IPostRepository _postRepository;
        private readonly INotificationService _notificationService;

        public LikeService(
            ILikeRepository likeRepository,
            IPostRepository postRepository,
            INotificationService notificationService)
        {
            _likeRepository = likeRepository;
            _postRepository = postRepository;
            _notificationService = notificationService;
        }

        public async Task<ServiceResult<bool>> ToggleLikeAsync(int postId, int userId)
        {
            var postExists = await _postRepository.PostExistsAsync(postId);

            if (!postExists)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "POST_NOT_FOUND",
                    "Post no encontrado."
                );

            var existingLike = await _likeRepository.GetByPostAndUserAsync(postId, userId);

            if(existingLike != null)
            {
                await _likeRepository.RemoveAsync(existingLike);
                return ServiceResult<bool>.Ok(false);
            }

            var like = new Like
            {
                PostId = postId,
                UserId = userId
            };

            var created = await _likeRepository.AddAsync(like);

            if(!created)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status409Conflict,
                    "DUPLICATED_LIKE",
                    "El usuario ya le dio like a esta publicación."
                );

            await _notificationService.CreateLikeNotificationAsync(userId, postId);

            return ServiceResult<bool>.Ok(true);
        }
    }
}
