using Learnify.Entities.Models;
using Learnify.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Entities.Interfaces
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<IEnumerable<Course>> GetAllWithInstructorAsync();
        Task<Course?> GetByIdWithInstructorAsync(int id);
    }

}
