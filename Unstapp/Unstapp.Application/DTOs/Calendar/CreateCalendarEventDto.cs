using System.ComponentModel.DataAnnotations;
using Unstapp.Infrastructure.Entities.Enums;

namespace Unstapp.Application.DTOs.Calendar
{
    public class CreateCalendarEventDto
    {
        [Required(ErrorMessage = "E título es obligatorio.")]
        [MaxLength(150, ErrorMessage = "El título no puede exceder los 150 caracteres.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "El tipo de evento es obligatorio.")]
        public CalendarEventType Type { get; set; }

        [Required(ErrorMessage = "La fecha y hora de inicio es obligatoria.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La fecha y hora de fin es obligatoria.")]
        public DateTime EndDate { get; set; }
        public List<int>? CareerIds { get; set; }
    }
}
