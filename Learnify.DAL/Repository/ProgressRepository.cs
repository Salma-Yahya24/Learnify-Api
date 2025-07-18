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

public class ProgressRepository : Repository<Progress>, IProgressRepository
{
    private readonly AppDbContext _context;

    public ProgressRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Progress?> GetByUserAndCourseAsync(int userId, int courseId)
    {
        return await _context.Progresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == courseId);
    }

}