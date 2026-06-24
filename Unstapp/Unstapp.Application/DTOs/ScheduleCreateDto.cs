using System.ComponentModel.DataAnnotations;

namespace Unstapp.Application.DTOs
{
    public class ScheduleCreateDto
    {
        [Required(ErrorMessage = "El ID de la carrera es obligatorio.")]
        public int CareerId { get; set; }

        [Required(ErrorMessage = "El nombre de la materia es obligatorio.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "El día de la semana es obligatorio.")]
        public string Day { get; set; } = string.Empty;

        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        public string StartTime { get; set; } = string.Empty; 

        public string Professor { get; set; } = string.Empty;
        public string Classroom { get; set; } = string.Empty;
        
        public decimal DurationHours { get; set; }
    }
}