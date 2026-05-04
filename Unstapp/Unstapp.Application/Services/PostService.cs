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
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        public PostService(IPostRepository postRepository, IMapper mapper)
        {
            _postRepository = postRepository;
            _mapper = mapper;
        }

        public async Task<PostDto> CreateAsync(int userId, CreatePostDto dto)
        {
            var post = _mapper.Map<Post>(dto);
            post.UserId = userId;
            post.PostDate = DateTime.UtcNow;

            await _postRepository.AddAsync(post);

            var createdPost = await _postRepository.GetByIdWithRelationsAsync(post.PostId);

            return _mapper.Map<PostDto>(createdPost!);
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
