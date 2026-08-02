using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task AddAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetValidTokenAsync(string tokenHash);
        Task MarkAsUsedAsync(PasswordResetToken token);
        Task InvalidateActiveTokensByUserIdAsync(int userId);
    }
}
