namespace Unstapp.Application.DTOs
{
    public class CareerAdminResponseDto
    {
        public int CareerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int FacultyId { get; set; }
        public string FacultyName { get; set; } = string.Empty;
    }
}