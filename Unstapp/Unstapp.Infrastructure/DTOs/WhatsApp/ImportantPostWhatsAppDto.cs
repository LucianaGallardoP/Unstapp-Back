namespace Unstapp.Infrastructure.DTOs.WhatsApp
{
    public class ImportantPostWhatsAppDto
    {
        public int PostId { get; set; }
        public List<int> CareerIds { get; set; } = new();
        public List<string> CareerNames { get; set; } = new();
        public string Content { get; set; } = string.Empty;
        public DateTime PostDate { get; set; }
    }
}
