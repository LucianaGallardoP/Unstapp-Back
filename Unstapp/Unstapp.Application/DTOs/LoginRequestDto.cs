using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class LoginRequestDto
    {
        public string Dni { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
