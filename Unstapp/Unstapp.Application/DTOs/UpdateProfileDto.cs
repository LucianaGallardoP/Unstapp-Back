using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class UpdateProfileDto
    {
        [MaxLength(500, ErrorMessage = "La biografía no puede superar los 500 caracteres.")]
        public string? Bio {  get; set; }
        public IFormFile? AvatarFile { get; set; }
        public IFormFile? CoverFile { get; set; }
        public bool RemoveBio { get; set; } = false;
        public bool RemoveAvatar { get; set; } = false;
        public bool RemoveCover { get; set; } = false;
    }
}
