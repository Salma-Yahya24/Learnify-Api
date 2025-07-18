using Learnify.DAL.Data;
using Learnify.Entities.Interfaces;
using Learnify.Entities.Models;
using Learnify.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.DAL.Repository
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Course>> GetAllWithInstructorAsync()
        {
            return await _context.Courses.Include(c => c.Instructor).ToListAsync();
        }

        public async Task<Course?> GetByIdWithInstructorAsync(int id)
        {
            return await _context.Courses.Include(c => c.Instructor)
                                         .FirstOrDefaultAsync(c => c.CourseId == id);
        }
    }

}
