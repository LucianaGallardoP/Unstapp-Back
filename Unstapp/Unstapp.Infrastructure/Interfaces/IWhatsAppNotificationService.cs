namespace Unstapp.Infrastructure.Interfaces
{
    public interface IWhatsAppNotificationService
    {
        Task NotifyImportantPostAsync(int postId);
    }
}
