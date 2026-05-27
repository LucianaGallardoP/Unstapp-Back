using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Data;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Entities.Enums;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.Helpers;

namespace Unstapp.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;

        public PostRepository(AppDbContext context, IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        public async Task AddAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task<Post?> GetByIdWithRelationsAsync(int postId)
        {
            return await _context.Posts
                .Where(p => !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.PostCareers)
                .FirstOrDefaultAsync(p => p.PostId == postId);
        }

        public async Task<List<Post>> GetAllWithRelationsAsync()
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.PostCareers)
                .OrderByDescending(p => p.PostDate)
                .ToListAsync();
        }

        public async Task<List<Post>> GetFilteredPostsAsync(int userId, PostFilter filter)
        {
            var query = _context.Posts
                .Where(p => !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.PostCareers)
                .AsQueryable();

            if(filter == PostFilter.MiCarrera)
            {
                var userCareerIds = await _userRepository.GetCareerIdsByUserIdAsync(userId);

                query = query.Where(p =>
                    p.Category == PostCategory.General
                    && p.PostCareers.Any(pc => userCareerIds.Contains(pc.CareerId)));
            }

            if(filter == PostFilter.Administrativo)
            {
                query = query.Where(p => p.Category == PostCategory.Administrativo);
            }

            return await query
                .OrderByDescending(p => p.PostDate)
                .ToListAsync();
        }

        public async Task<bool> PostExistsAsync(int postId)
        {
            return await _context.Posts.AnyAsync(p => p.PostId == postId);
        }

        public async Task<List<Post>> SearchPostsAsync(string term)
        {
            term = SearchHelpers.RemoveDiacritics(term.Trim());

            return await _context.Posts
                .Where(p =>
                    !p.IsDeleted &&
                    p.Content != null &&
                    EF.Functions.ILike(PostgresDbFunctions.Unaccent(p.Content), $"%{term}%"))
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.PostDate)
                .Take(20)
                .ToListAsync();
        }

        public async Task<Post?> GetByIdIncludingDeletedAsync(int postId)
        {
            return await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == postId);
        }

        public async Task SoftDeleteAsync(Post post)
        {
            post.IsDeleted = true;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }
    }
}
