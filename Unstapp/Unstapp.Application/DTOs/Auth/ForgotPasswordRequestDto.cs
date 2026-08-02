using System.ComponentModel.DataAnnotations;

namespace Unstapp.Application.DTOs.Auth
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        public string DNI { get; set; } = string.Empty;
    }
}
