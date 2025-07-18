using Learnify.Entities.Interfaces;
using Learnify.Entities.Models;
using System;
using System.Threading.Tasks;

namespace Learnify.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRepository<Role> Roles { get; }
        ICourseRepository Courses { get; }
        IEnrollmentRepository Enrollments { get; }
        ILessonRepository LessonRepository { get; }
        ILessonCompletionRepository LessonCompletions { get; } // ✅ أضفنا هنا
        IProgressRepository ProgressRepository { get; }

        
        IRepository<Gender> Genders { get; }

        Task<int> SaveAsync();
    }
}
