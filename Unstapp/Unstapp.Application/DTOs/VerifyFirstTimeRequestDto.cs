using System.ComponentModel.DataAnnotations;

namespace Unstapp.Application.DTOs
{
    public class VerifyFirstTimeRequestDto
    {
        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "El DNI debe contener solo números.")]
        [StringLength(8, MinimumLength = 7, ErrorMessage = "El DNI debe contener entre 7 y 8 dígitos.")]
        public string DNI { get; set; } = null!;
    }
}
