using Microsoft.EntityFrameworkCore;
using Unstapp.Infrastructure.Data;
using Unstapp.Infrastructure.DTOs.WhatsApp;
using Unstapp.Infrastructure.Entities.Enums;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.Helpers;

namespace Unstapp.Infrastructure.Repositories
{
    public class WhatsAppNotificationRepository : IWhatsAppNotificationRepository
    {
        private readonly AppDbContext _context;

        public WhatsAppNotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ImportantPostWhatsAppDto?> GetImportantPostAsync(int postId)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .Include(p => p.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .Where(p =>
                    p.PostId == postId &&
                    p.IsImportant &&
                    !p.IsDeleted
                )
                .FirstOrDefaultAsync();

            if (post == null)
                return null;

            var careerIds = await _context.PostCareers
                .AsNoTracking()
                .Where(pc => pc.PostId == postId)
                .Select(pc => pc.CareerId)
                .Distinct()
                .ToListAsync();

            var careerNames = new List<string>();

            if(careerIds.Count > 0)
            {
                careerNames = await _context.Careers
                    .AsNoTracking()
                    .Where(c => careerIds.Contains(c.CareerId))
                    .OrderBy(c => c.Name)
                    .Select(c => c.Name)
                    .ToListAsync();
            }

            var roleNames = post.User.UserRoles
                .Select(ur => ur.Role.Name)
                .ToList();

            var senderName = BuildSenderName(
                post.User.Name,
                post.User.LastName,
                roleNames
            );

            return new ImportantPostWhatsAppDto
            {
                PostId = post.PostId,
                Content = post.Content ?? "Nuevo aviso importante",
                PostDate = post.PostDate,
                CareerIds = careerIds,
                CareerNames = careerNames,
                SenderName = senderName
            };
        }

        public async Task<List<WhatsAppRecipientDto>> GetStudentsWithWhatsAppEnabledByCareerIdsAsync(List<int> careerIds)
        {
            if (careerIds.Count == 0)
                return new List<WhatsAppRecipientDto>();

            return await _context.UserCareers
                .AsNoTracking()
                .Where(uc =>
                    careerIds.Contains(uc.CareerId) &&
                    uc.User.WhatsAppNotificationsEnabled &&
                    uc.User.PhoneNumber != null &&
                    uc.User.UserRoles.Any(ur => ur.Role.Name == "Alumno")
                )
                .GroupBy(uc => new
                {
                    uc.User.UserId,
                    uc.User.Name,
                    uc.User.LastName,
                    uc.User.PhoneNumber
                })
                .Select(g => new WhatsAppRecipientDto
                {
                    UserId = g.Key.UserId,
                    FullName = g.Key.Name + " " + g.Key.LastName,
                    PhoneNumber = g.Key.PhoneNumber!
                })
                .ToListAsync();
        }

        public async Task<List<WhatsAppRecipientDto>> GetStudentsWithWhatsAppEnabledAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u =>
                    u.WhatsAppNotificationsEnabled &&
                    u.PhoneNumber != null &&
                    u.UserRoles.Any(ur => ur.Role.Name == "Alumno")
                )
                .Select(u => new WhatsAppRecipientDto
                {
                    UserId = u.UserId,
                    FullName = u.Name + " " + u.LastName,
                    PhoneNumber = u.PhoneNumber!
                })
                .ToListAsync();
        }

        private static string BuildSenderName(string? name, string? lastName, List<string> roles)
        {
            var fullName = $"{name} {lastName}".Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                fullName = "Unstapp";

            var isProffesor = RoleHelper.IsProffesor(roles);

            if (isProffesor)
                return $"Prof. {fullName}";

            return fullName;
        }
    }
}
