namespace Unstapp.Infrastructure.DTOs.WhatsApp
{
    public class WhatsAppRecipientDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
