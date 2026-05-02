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

        public LikeService(ILikeRepository likeRepository)
        {
            _likeRepository = likeRepository;
        }

        public async Task<ToggleLikeResult> ToggleLikeAsync(int postId, int userId)
        {
            var postExists = await _likeRepository.PostExistsAsync(postId);

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

            await _likeRepository.AddAsync(like);
            return ToggleLikeResult.Liked;
        }
    }
}
