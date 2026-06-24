using System.ComponentModel.DataAnnotations;

namespace Unstapp.Application.DTOs
{
    public class SetInitialPasswordRequestDto
    {
        [Required(ErrorMessage = "El token es obligatorio.")]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
