using Microsoft.EntityFrameworkCore;
using Unstapp.Infrastructure.Data;
using Unstapp.Infrastructure.DTOs.WhatsApp;
using Unstapp.Infrastructure.Entities.Enums;
using Unstapp.Infrastructure.Interfaces;

namespace Unstapp.Infrastructure.Repositories
{
    public class WhatsAppNotificationRepository : IWhatsAppNotificationRepository
    {
        private readonly AppDbContext _context;

        public WhatsAppNotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ImportantPostWhatsAppDto?> GetImportantAdministrationPostAsync(int postId)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .Where(p =>
                    p.PostId == postId &&
                    p.Category == PostCategory.Administrativo &&
                    p.IsImportant &&
                    p.User.UserRoles.Any(ur => ur.Role.Name == "Administracion")
                )
                .Select(p => new
                {
                    p.PostId,
                    Content = p.Content ?? "Nuevo aviso importante",
                    p.PostDate
                })
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

            return new ImportantPostWhatsAppDto
            {
                PostId = post.PostId,
                Content = post.Content,
                PostDate = post.PostDate,
                CareerIds = careerIds,
                CareerNames = careerNames
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
    }
}
