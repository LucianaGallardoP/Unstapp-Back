namespace Unstapp.Infrastructure.Interfaces
{
    public interface IWhatsAppNotificationDispatcher
    {
        void DispatchImportantPostNotification(int postId);
    }
}
