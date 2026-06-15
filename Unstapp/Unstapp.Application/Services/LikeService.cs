using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;

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

        public async Task<ToggleLikeResult> ToggleLikeAsync(int postId, int userId)
        {
            var postExists = await _postRepository.PostExistsAsync(postId);

            if (!postExists)
                return ToggleLikeResult.PostNotFound;

            var existingLike = await _likeRepository.GetByPostAndUserAsync(postId, userId);

            if(existingLike != null)
            {
                await _likeRepository.RemoveAsync(existingLike);
                return ToggleLikeResult.Unliked;
            }

            var like = new Like
            {
                PostId = postId,
                UserId = userId
            };

            var created = await _likeRepository.AddAsync(like);

            if(!created)
                return ToggleLikeResult.DuplicateLike;

            await _notificationService.CreateLikeNotificationAsync(userId, postId);

            return ToggleLikeResult.Liked;
        }
    }
}
