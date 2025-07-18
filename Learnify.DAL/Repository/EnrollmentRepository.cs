using Learnify.DAL.Data;
using Learnify.Entities.Interfaces;
using Learnify.Entities.Models;
using Learnify.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learnify.DAL.Repository
{
    public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
    {
        private readonly AppDbContext _context;

        public EnrollmentRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Enrollment>> GetAllWithDetailsAsync()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .ToListAsync();

            // تسجيل البيانات المرتبطة
            foreach (var enrollment in enrollments)
            {
                Console.WriteLine($"User: {enrollment.User?.UserName}, Course: {enrollment.Course?.Title}");
            }

            return enrollments;
        }

        public async Task<Enrollment?> GetByIdWithDetailsAsync(int id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            // تسجيل البيانات المرتبطة
            if (enrollment != null)
            {
                Console.WriteLine($"User: {enrollment.User?.UserName}, Course: {enrollment.Course?.Title}");
            }

            return enrollment;
        }

        public async Task<bool> IsUserEnrolledAsync(int userId, int courseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
        }

        public async Task<IEnumerable<Enrollment>> GetByUserIdAsync(int userId)
        {
            return await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                .ToListAsync();
        }

        public async Task UpdateProgressAsync(int enrollmentId, float progress)
        {
            var existingProgress = await _context.Progresses
                .FirstOrDefaultAsync(p => p.EnrollmentId == enrollmentId);

            if (existingProgress != null)
            {
                existingProgress.CompletionPercentage = progress;
                existingProgress.LastUpdated = DateTime.UtcNow;
                _context.Progresses.Update(existingProgress);
            }
            else
            {
                var newProgress = new Progress
                {
                    EnrollmentId = enrollmentId,
                    CompletionPercentage = progress,
                    LastUpdated = DateTime.UtcNow
                };
                await _context.Progresses.AddAsync(newProgress);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Enrollment?> GetByUserAndCourseAsync(int userId, int courseId)
        {
            return await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        }
    }
}
