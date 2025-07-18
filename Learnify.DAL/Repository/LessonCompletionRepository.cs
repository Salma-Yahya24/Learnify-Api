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
    public class LessonCompletionRepository : Repository<LessonCompletion>, ILessonCompletionRepository
    {
        private readonly AppDbContext _context;

        public LessonCompletionRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LessonCompletion>> GetCompletedLessonsByUserAndCourseAsync(int userId, int courseId)
        {
            return await _context.LessonCompletions
                .Include(lc => lc.Lesson)
                .Where(lc => lc.UserId == userId && lc.Lesson!.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<bool> IsLessonCompletedAsync(int userId, int lessonId)
        {
            return await _context.LessonCompletions
                .AnyAsync(lc => lc.UserId == userId && lc.LessonId == lessonId);
        }
    }
}
