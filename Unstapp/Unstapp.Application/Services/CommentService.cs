using AutoMapper;
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
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public CommentService(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            INotificationService notificationService,
            IMapper mapper)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<List<CommentResponseDto>?> GetAllByPostAsync(int postId)
        {
            var postExists = await _postRepository.PostExistsAsync(postId);

            if (!postExists)
                return null;

            var comments = await _commentRepository.GetAllByPostWithRelationsAsync(postId);

            var commentDtos = _mapper.Map<List<CommentResponseDto>>(comments);

            return commentDtos;
        }

        public async Task<CommentResponseDto?> AddAsync(
            int postId,
            int userId,
            CreateCommentDto dto)
        {
            var postExists = await _postRepository.PostExistsAsync(postId);
            if (!postExists)
                return null;

            var comment = _mapper.Map<Comment>(dto);
            comment.UserId = userId;
            comment.PostId = postId;
            comment.CreatedAt = DateTime.UtcNow;

            await _commentRepository.AddAsync(comment);

            await _notificationService.CreateCommentNotificationAsync(userId, postId);

            var createdComment = await _commentRepository.GetByIdWithRelationsAsync(comment.CommentId);

            return _mapper.Map<CommentResponseDto>(createdComment!);
        }
    }
}
