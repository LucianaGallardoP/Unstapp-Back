using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Interfaces;

namespace Unstapp.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;

        public CommentService(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            IMapper mapper)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _mapper = mapper;
        }

        public async Task<List<CommentResponseDto>?> GetAllByPostAsync(int postId)
        {
            var postExists = await _postRepository.PostExistsAsync(postId);

            if (!postExists)
                return null;

            var comments = await _commentRepository.GetAllByPostWithRelationsAsync(postId);

            var postDtos = _mapper.Map<List<CommentResponseDto>>(comments);

            return postDtos;
        }
    }
}
