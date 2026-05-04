using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface ICommentRepository
    {
        public Task<List<Comment>> GetAllByPostWithRelationsAsync(int postId);
    }
}
