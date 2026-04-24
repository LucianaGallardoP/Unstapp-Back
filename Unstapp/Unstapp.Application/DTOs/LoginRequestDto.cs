using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "El DNI debe contener solo números.")]
        [StringLength(8, MinimumLength = 7, ErrorMessage = "El DNI debe contener entre 7 y 8 dígitos.")]
        public string DNI { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = null!;
    }
}
