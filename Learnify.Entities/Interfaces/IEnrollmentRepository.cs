using Learnify.Entities.Models;
using Learnify.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Learnify.Entities.Interfaces
{
    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        Task<IEnumerable<Enrollment>> GetAllWithDetailsAsync();
        Task<Enrollment?> GetByIdWithDetailsAsync(int id);

        // Check if user is already enrolled
        Task<bool> IsUserEnrolledAsync(int userId, int courseId);

        // Get all enrollments by user
        Task<IEnumerable<Enrollment>> GetByUserIdAsync(int userId);

        // Update progress for enrollment
        Task UpdateProgressAsync(int enrollmentId, float progress);

        // Get enrollment by user and course (for lesson access validation)
        Task<Enrollment?> GetByUserAndCourseAsync(int userId, int courseId);
    }
}
