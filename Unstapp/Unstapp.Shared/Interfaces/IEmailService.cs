namespace Unstapp.Shared.Interfaces
{
    public interface IEmailService
    {
        Task SendFirstLoginEmailAsync(string toEmail, string fullName, string confirmationLink);
        Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink, int expirationMinutes);
    }
}
