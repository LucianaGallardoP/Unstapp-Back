using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Services
{
    public class SearchService : ISearchService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;

        public SearchService(
            IUserRepository userRepository,
            IPostRepository postRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<SearchResponseDto>> SearchAsync(string term, int userId)
        {
            if (string.IsNullOrWhiteSpace(term))
                return ServiceResult<SearchResponseDto>.Ok(new SearchResponseDto
                {
                    Users = new List<UserSearchResponseDto>(),
                    Posts = new List<PostDto>()
                });

            term = term.Trim();

            var users = await _userRepository.SearchUsersAsync(term);
            var posts = await _postRepository.SearchPostsAsync(term);

            var usersDto = users.Select(u => new UserSearchResponseDto
            {
                UserId = u.UserId,
                UserName = $"{u.Name} {u.LastName}".Trim(),
                AvatarUrl = u.AvatarUrl,
            }).ToList();

            var postsDto = _mapper.Map<List<PostDto>>(posts);

            foreach(var postDto in postsDto)
            {
                var post = posts.First(p => p.PostId == postDto.PostId);
                postDto.isLikedByMe = post.Likes.Any(l => l.UserId == userId);
            }

            var response = new SearchResponseDto
            {
                Users = usersDto,
                Posts = postsDto,
            };

            return ServiceResult<SearchResponseDto>.Ok(response);
        }
    }
}
