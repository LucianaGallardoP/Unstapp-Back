using Microsoft.EntityFrameworkCore;
using Unstapp.Infrastructure.Data;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;

namespace Unstapp.Infrastructure.Repositories
{
    public class FirstLoginTokenRepository : IFirstLoginTokenRepository
    {
        private readonly AppDbContext _context;

        public FirstLoginTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(FirstLoginToken token)
        {
            await _context.FirstLoginTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task InvalidatePreviousAsync(int userId)
        {
            var pending = await _context.FirstLoginTokens
                .Where(t => t.UserId == userId && !t.Used)
                .ToListAsync();

            foreach (var token in pending)
            {
                token.Used = true;
            }

            if (pending.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<FirstLoginToken?> GetActiveByHashAsync(string tokenHash)
        {
            return await _context.FirstLoginTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.TokenHash == tokenHash &&
                    !t.Used &&
                    t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
