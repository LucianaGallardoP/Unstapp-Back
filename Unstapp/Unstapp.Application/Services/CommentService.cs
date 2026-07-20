using AutoMapper;
using Microsoft.AspNetCore.Http;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;
using Unstapp.Shared.Helpers;

namespace Unstapp.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CommentService(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            INotificationService notificationService,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<List<CommentResponseDto>?>> GetAllByPostAsync(int postId)
        {
            var postExists = await _postRepository.PostExistsAsync(postId);

            if (!postExists)
                return ServiceResult<List<CommentResponseDto>?>.Fail(
                    StatusCodes.Status404NotFound,
                    "POST_NOT_FOUND",
                    "Post no encontrado."
                );

            var comments = await _commentRepository.GetAllByPostWithRelationsAsync(postId);

            var commentDtos = _mapper.Map<List<CommentResponseDto>>(comments);

            return ServiceResult<List<CommentResponseDto>?>.Ok(commentDtos);
        }

        public async Task<ServiceResult<CommentResponseDto>> AddAsync(
            int postId,
            int userId,
            CreateCommentDto dto)
        {
            var postExists = await _postRepository.PostExistsAsync(postId);

            if (!postExists)
                return ServiceResult<CommentResponseDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "POST_NOT_FOUND",
                    "Post no encontrado."
                );

            var comment = _mapper.Map<Comment>(dto);
            comment.UserId = userId;
            comment.PostId = postId;
            comment.CreatedAt = DateTime.UtcNow;

            await _commentRepository.AddAsync(comment);

            await _notificationService.CreateCommentNotificationAsync(userId, postId);

            var createdComment = await _commentRepository.GetByIdWithRelationsAsync(comment.CommentId);

            var responseDto = _mapper.Map<CommentResponseDto>(createdComment);

            return ServiceResult<CommentResponseDto>.Ok(responseDto);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int commentId, int currentUserId)
        {
            var comment = await _commentRepository.GetByIdWithRelationsAsync(commentId);

            if (comment == null)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "COMMENT_NOT_FOUND",
                    "Comentario no encontrado."
                );

            var roles = await _userRepository.GetRoleNameByUserIdAsync(currentUserId);

            var isAdmin = RoleHelper.IsAdmin(roles);

            var isCommentAuthor = comment.UserId == currentUserId;
            var isPostOwner = comment.Post.UserId == currentUserId;

            if (!isCommentAuthor && !isPostOwner && !isAdmin)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status403Forbidden,
                    "FORBIDDEN_COMMENT_DELETE",
                    "No tienes permiso para eliminar este comentario."
                );

            await _commentRepository.DeleteAsync(comment);
            return ServiceResult<bool>.Ok(true);
        }
    }
}
