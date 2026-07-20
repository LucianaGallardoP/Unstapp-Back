using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.DTOs.WhatsApp;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface IWhatsAppNotificationRepository
    {
        Task<ImportantPostWhatsAppDto?> GetImportantAdministrationPostAsync(int postId);
        Task<List<WhatsAppRecipientDto>> GetStudentsWithWhatsAppEnabledByCareerIdsAsync(List<int> careerIds);
        Task<List<WhatsAppRecipientDto>> GetStudentsWithWhatsAppEnabledAsync();
    }
}
