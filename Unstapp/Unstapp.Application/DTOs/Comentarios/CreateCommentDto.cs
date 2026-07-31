
using System.ComponentModel.DataAnnotations;

namespace Unstapp.Application.DTOs.Comentarios
{
    public class CreateCommentDto
    {
        [Required(ErrorMessage = "El contenido del comentario es obligatorio.")]
        [MaxLength(300, ErrorMessage = "El contenido del comentario no puede superar los 300 caracteres.")]
        public string Content { get; set; } = null!;
    }
}
