using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class SearchResponseDto
    {
        public List<UserSearchResponseDto> Users { get; set; } = new();
        public List<PostDto> Posts { get; set; } = new();
    }
}
