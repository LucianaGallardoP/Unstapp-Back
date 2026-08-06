using AutoMapper;
using Microsoft.AspNetCore.Http;
using Unstapp.Application.DTOs;
using Unstapp.Application.DTOs.Posts;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Entities.Enums;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;
using Unstapp.Shared.Helpers;
using Unstapp.Shared.Interfaces;

namespace Unstapp.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly IMediaStorageService _mediaStorageService;
        private readonly IUserRepository _userRepository;
        private readonly ICareerRepository _careerRepository;
        public PostService(
            IPostRepository postRepository,
            IMapper mapper,
            IMediaStorageService mediaStorageService,
            IUserRepository userRepository,
            ICareerRepository careerRepository)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _mediaStorageService = mediaStorageService;
            _userRepository = userRepository;
            _careerRepository = careerRepository;
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

            var isAdmin = RoleHelper.IsAdmin(roles);

            var isProfessor = RoleHelper.IsProffesor(roles);

            var canCreateImportantPost = isAdmin || isProfessor;

            if (dto.IsImportant && !canCreateImportantPost)
                return ServiceResult<PostDto>.Fail(
                    StatusCodes.Status403Forbidden,
                    "IMPORTANT_POST_NOT_ALLOWED",
                    "No tenés permisos para crear avisos importantes."
                );

            var requestedCareerIds = dto.CareerIds?.Distinct().ToList() ?? new List<int>();

            if (requestedCareerIds.Any(id => id <= 0))
                return ServiceResult<PostDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_CAREER_ID",
                    "Los identificadores de carrera no son válidos."
                );

            var postCareerIds = new List<int>();

            if (category == PostCategory.General)
            {
                var userCareerIds = await _userRepository.GetCareerIdsByUserIdAsync(userId);

                userCareerIds = userCareerIds.Distinct().ToList();

                if (isProfessor && dto.IsImportant)
                {
                    if (userCareerIds.Count == 0)
                        return ServiceResult<PostDto>.Fail(
                            StatusCodes.Status400BadRequest,
                            "TEACHER_WITHOUT_CAREERS",
                            "El docente no tiene carreras asignadas."
                        );

                    if (requestedCareerIds.Count == 0)
                    {
                        postCareerIds = userCareerIds;
                    }
                    else
                    {
                        var unauthorizedCareerIds = requestedCareerIds.Except(userCareerIds).ToList();

                        if (unauthorizedCareerIds.Count > 0)
                        {
                            return ServiceResult<PostDto>.Fail(
                                StatusCodes.Status403Forbidden,
                                "CAREER_NOT_ASSIGNED_TO_TEACHER",
                                "No podés enviar el aviso a una carrera que no tenés asignada."
                            );
                        }

                        postCareerIds = requestedCareerIds;
                    }
                }
                else
                {
                    postCareerIds = userCareerIds;
                }
            }
            else
            {
                postCareerIds = requestedCareerIds;

                if (postCareerIds.Count > 0)
                {
                    var existingCareerIds = await _careerRepository.GetExistingCareerIdsAsync(postCareerIds);

                    var missingCareerIds = postCareerIds
                        .Except(existingCareerIds)
                        .ToList();

                    if (missingCareerIds.Count > 0)
                        return ServiceResult<PostDto>.Fail(
                            StatusCodes.Status400BadRequest,
                            "CAREER_NOT_FOUND",
                            $"Las siguientes carreras no existen: {string.Join(", ", missingCareerIds)}."
                        );
                }
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

            post.Content = string.IsNullOrWhiteSpace(dto.Content)
                ? null
                : dto.Content.Trim();

            post.UserId = userId;
            post.PostDate = DateTime.UtcNow;
            post.MediaUrl = mediaUrl;
            post.Category = category;
            post.IsImportant = canCreateImportantPost && dto.IsImportant;

            await _postRepository.AddPostWithCareersAsync(post, postCareerIds);

            var createdPost = await _postRepository.GetByIdWithRelationsAsync(post.PostId);

            if (createdPost == null)
                return ServiceResult<PostDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "POST_NOT_FOUND",
                    "No se pudo recuperar el post creado."
                );

            var responseDto = _mapper.Map<PostDto>(createdPost);

            responseDto.AuthorRoleName = ResolveDisplayRole(roles);

            return ServiceResult<PostDto>.Ok(responseDto);
        }

        public async Task<ServiceResult<PaginatedPostsResponseDto>> GetAllAsync(int userId, PostFilter filter, int page, int limit)
        {

            if(userId <= 0)
                return ServiceResult<PaginatedPostsResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_USER_ID",
                    "El id del usuario no es válido."
                );

            if(page <= 0)
                page = 1;

            if(limit <= 0)
                limit = 15;

            if (limit > 50)
                limit = 50;

            var result = await _postRepository.GetFilteredPostsAsync(userId, filter, page, limit);

            var postDtos = result.Posts.Select(post =>
            {
                var dto = _mapper.Map<PostDto>(post);

                dto.AuthorRoleName = ResolveDisplayRole(
                    post.User.UserRoles.Select(Ur => Ur.Role.Name)
                );

                dto.LikesCount = post.Likes.Count;
                dto.CommentsCount = post.Comments.Count;
                dto.isLikedByMe = post.Likes.Any(l => l.UserId == userId);

                return dto;
            }).ToList();

            var response = new PaginatedPostsResponseDto
            {
                Posts = postDtos,
                HasMore = result.HasMore
            };

            return ServiceResult<PaginatedPostsResponseDto>.Ok(response);
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

            postDto.AuthorRoleName = ResolveDisplayRole(
                post.User.UserRoles.Select(ur => ur.Role.Name)
            );

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

            var isAdmin = RoleHelper.IsAdmin(roles);

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
            if (RoleHelper.IsAdmin(roles))
            {
                return PostCategory.Administrativo;
            }

            return PostCategory.General;
        }
        
        public async Task<ServiceResult<List<PostDto>>> GetPostsByUserAsync(int userId)
        {
            var userExists = await _userRepository.ExistsAsync(userId);

            if(!userExists)
                return ServiceResult<List<PostDto>>.Fail(
                    StatusCodes.Status404NotFound,
                    "USER_NOT_FOUND",
                    "Usuario no encontrado."
                );

            var posts = await _postRepository.GetPostsByUserAsync(userId);

            var postDtos = _mapper.Map<List<PostDto>>(posts);

            foreach (var postDto in postDtos)
            {
                var post = posts.First(p => p.PostId == postDto.PostId);

                postDto.AuthorRoleName = ResolveDisplayRole(
                    post.User.UserRoles.Select(ur => ur.Role.Name)
                );

                postDto.LikesCount = post.Likes.Count;
                postDto.CommentsCount = post.Comments.Count;
                postDto.isLikedByMe = post.Likes.Any(l => l.UserId == userId);
            }

            return ServiceResult<List<PostDto>>.Ok(postDtos);
        }

        private static string ResolveDisplayRole(IEnumerable<string> roles)
        {
            var roleList = roles
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .ToList();

            if (RoleHelper.IsAdmin(roleList))
                return "Administracion";

            if (RoleHelper.IsProffesor(roleList))
                return "Docente";

            if (roleList.Any(r => r.Equals("Alumno", StringComparison.OrdinalIgnoreCase)))
                return "Alumno";

            if (roleList.Any(r => r.Equals("Bar", StringComparison.OrdinalIgnoreCase)))
                return "Bar";

            if (roleList.Any(r => r.Equals("Fotocopiadora", StringComparison.OrdinalIgnoreCase)))
                return "Fotocopiadora";

            if (roleList.Any(r => r.Equals("Biblioteca", StringComparison.OrdinalIgnoreCase)))
                return "Biblioteca";

            return "Usuario";
        }
    }
}
