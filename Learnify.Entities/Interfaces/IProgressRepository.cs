using Learnify.Entities.Models;
using Learnify.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Entities.Interfaces
{
    public interface IProgressRepository : IRepository<Progress>
    {
        Task<Progress?> GetByUserAndCourseAsync(int userId, int courseId);
    }
}
