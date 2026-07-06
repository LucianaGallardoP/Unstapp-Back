using Unstapp.Shared.DTOs.Moderation;

namespace Unstapp.Shared.Interfaces
{
    public interface IModerationService
    {
        Task<ModerationResultDto> ModeratePostAsync(string? Content);
    }
}
