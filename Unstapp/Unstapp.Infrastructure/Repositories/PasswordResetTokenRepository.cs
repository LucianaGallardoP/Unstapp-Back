using Microsoft.EntityFrameworkCore;
using Unstapp.Infrastructure.Data;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;

namespace Unstapp.Infrastructure.Repositories
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly AppDbContext _context;

        public PasswordResetTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PasswordResetToken token)
        {
            await _context.PasswordResetTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<PasswordResetToken?> GetValidTokenAsync(string tokenHash)
        {
            return await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.TokenHash == tokenHash &&
                    t.UsedAt == null &&
                    t.ExpiresAt > DateTime.UtcNow
                );
        }

        public async Task MarkAsUsedAsync(PasswordResetToken token)
        {
            token.UsedAt = DateTime.UtcNow;

            _context.PasswordResetTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task InvalidateActiveTokensByUserIdAsync(int userId)
        {
            var activeTokens = await _context.PasswordResetTokens
                .Where(t =>
                    t.UserId == userId &&
                    t.UsedAt == null &&
                    t.ExpiresAt > DateTime.UtcNow
                ).ToListAsync();

            foreach(var token in activeTokens)
            {
                token.UsedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
