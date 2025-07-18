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

namespace Learnify.DAL.Repository;

public class LessonRepository : Repository<Lesson>, ILessonRepository
{
    private readonly AppDbContext _context;

    public LessonRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Lesson>> GetLessonsByCourseIdAsync(int courseId)
    {
        return await _context.Lessons
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.Order)
            .ToListAsync();
    }
}
