using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Infrastructure.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
