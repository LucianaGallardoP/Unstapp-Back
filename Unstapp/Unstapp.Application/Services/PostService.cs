using AutoMapper;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.Interfaces;
using Unstapp.Shared.DTOs.Common;
using Microsoft.AspNetCore.Http;
using Npgsql.PostgresTypes;
using Unstapp.Infrastructure.Entities.Enums;

namespace Unstapp.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly IMediaStorageService _mediaStorageService;
        private readonly IUserRepository _userRepository;
        public PostService(
            IPostRepository postRepository,
            IMapper mapper,
            IMediaStorageService mediaStorageService,
            IUserRepository userRepository)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _mediaStorageService = mediaStorageService;
            _userRepository = userRepository;
        }

        public async Task<ServiceResult<PostDto>> CreateAsync(int userId, CreatePostDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content) && dto.MediaFile == null)
            {
                return ServiceResult<PostDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "POST_EMPTY",
                    "El post debe tener texto o archivo multimedia."
                    );
            }

            var roles = await _userRepository.GetRoleNameByUserIdAsync(userId);

            var category = ResolvePostCategoryFromRoles(roles);

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

            post.Content = string.IsNullOrWhiteSpace(dto.Content)
                ? null
                : dto.Content.Trim();

            post.UserId = userId;
            post.PostDate = DateTime.UtcNow;
            post.MediaUrl = mediaUrl;
            post.Category = category;

            if(post.Category == PostCategory.General)
            {
                var userCareerIds = await _userRepository.GetCareerIdsByUserIdAsync(userId);

                foreach(var careerId in userCareerIds)
                {
                    post.PostCareers.Add(new PostCareer
                    {
                        CareerId = careerId,
                    });
                }
            }

            await _postRepository.AddAsync(post);

            var createdPost = await _postRepository.GetByIdWithRelationsAsync(post.PostId);

            var responseDto = _mapper.Map<PostDto>(createdPost);

            return ServiceResult<PostDto>.Ok(responseDto);
        }

        public async Task<ServiceResult<List<PostDto>>> GetAllAsync(int userId, PostFilter filter)
        {
            var posts = await _postRepository.GetFilteredPostsAsync(userId, filter);

            var postDtos = _mapper.Map<List<PostDto>>(posts);

            foreach (var postDto in postDtos)
            {
                var post = posts.First(p => p.PostId == postDto.PostId);

                postDto.LikesCount = post.Likes.Count();
                postDto.CommentsCount = post.Comments.Count();
                postDto.isLikedByMe = post.Likes.Any(l => l.UserId == userId);
            }

            return ServiceResult<List<PostDto>>.Ok(postDtos);
        }

        public async Task<ServiceResult<PostDto>> GetByIdAsync(int userId, int postId)
        {
            var post = await _postRepository.GetByIdWithRelationsAsync(postId);

            if (post == null)
                return ServiceResult<PostDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "POST_NOT_FOUND",
                    "Publicación no encontrada."
                    );

            var postDto = _mapper.Map<PostDto>(post);
            postDto.isLikedByMe = post.Likes.Any(l => l.UserId == userId);

            return ServiceResult<PostDto>.Ok(postDto);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int postId, int currentUserId)
        {
            var post = await _postRepository.GetByIdIncludingDeletedAsync(postId);

            if (post == null || post.IsDeleted)
            {
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "POST_NOT_FOUND",
                    "Publicación no encontrada."
                );
            }

            var roles = await _userRepository.GetRoleNameByUserIdAsync(currentUserId);

            var isAdmin = roles.Any(r =>
                r.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("Administracion", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("Administrativo", StringComparison.OrdinalIgnoreCase)
            );

            var isOwner = post.UserId == currentUserId;

            if (!isOwner && !isAdmin)
            {
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status403Forbidden,
                    "FORBIDDEN_POST_DELETE",
                    "No tenés permiso para eliminar esta publicación."
                );
            }

            await _postRepository.SoftDeleteAsync(post);

            return ServiceResult<bool>.Ok(true);
        }

        private static PostCategory ResolvePostCategoryFromRoles(List<string> roles)
        {
            if (roles.Any(r =>
            r.Equals("Bar", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("Fotocopiadora", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("Administración", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("Administracion", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("Administrativo", StringComparison.OrdinalIgnoreCase)))
            {
                return PostCategory.Administrativo;
            }

            if(roles.Any(r =>
            r.Equals("Alumno", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("Profesor", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("Docente", StringComparison.OrdinalIgnoreCase)))
            {
                return PostCategory.General;
            }

            return PostCategory.General;
        }
        
        public async Task<List<PostDto>> GetPostsByUserAsync(int userId)
        {
            var posts = await _postRepository.GetPostsByUserAsync(userId);

            return _mapper.Map<List<PostDto>>(posts);
        }
    }
}
