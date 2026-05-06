using AutoMapper;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.Interfaces;
using Unstapp.Shared.DTOs.Common;
using Microsoft.AspNetCore.Http;
using Npgsql.PostgresTypes;

namespace Unstapp.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly IMediaStorageService _mediaStorageService;
        public PostService(
            IPostRepository postRepository,
            IMapper mapper,
            IMediaStorageService mediaStorageService)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _mediaStorageService = mediaStorageService;
        }

        public async Task<ServiceResult<PostDto>> CreateAsync(int userId, CreatePostDto dto)
        {
            if(string.IsNullOrWhiteSpace(dto.Content))
            {
                return ServiceResult<PostDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "POST_EMPTY",
                    "El post debe tener texto."
                    );
            }

            string? mediaUrl = null;

            if(dto.MediaFile != null)
            {
                var uploadResult = await _mediaStorageService.UploadPostMediaAsync(dto.MediaFile, userId);
                
                if(!uploadResult.Success)
                    return ServiceResult<PostDto>.Fail(
                        uploadResult.Error!.StatusCode,
                        uploadResult.Error.Code,
                        uploadResult.Error.Message
                        );
                mediaUrl = uploadResult.Data;
            }

            var post = _mapper.Map<Post>(dto);
            post.UserId = userId;
            post.PostDate = DateTime.UtcNow;
            post.MediaUrl = mediaUrl;

            await _postRepository.AddAsync(post);

            var createdPost = await _postRepository.GetByIdWithRelationsAsync(post.PostId);

            var responseDto = _mapper.Map<PostDto>(createdPost);

            return ServiceResult<PostDto>.Ok(responseDto);
        }

        public async Task<List<PostDto>> GetAllAsync(int currentUserId)
        {
            var posts = await _postRepository.GetAllWithRelationsAsync();

            var postDtos = _mapper.Map<List<PostDto>>(posts);

            foreach (var postDto in postDtos)
            {
                var post = posts.First(p => p.PostId == postDto.PostId);

                postDto.LikesCount = post.Likes.Count();
                postDto.CommentsCount = post.Comments.Count();
                postDto.isLikedByMe = post.Likes.Any(l => l.UserId == currentUserId);
            }

            return postDtos;
        }
    }
}
