using Learnify.DAL.Data;
using Learnify.DAL.Repository;
using Learnify.Entities.Interfaces;
using Learnify.Entities.Models;
using Learnify.Interfaces;
using Learnify.Repository;

namespace Learnify.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            Roles = new Repository<Role>(_context);
            Courses = new CourseRepository(_context);
            Enrollments = new EnrollmentRepository(_context);
            LessonRepository = new LessonRepository(_context);
            LessonCompletions = new LessonCompletionRepository(_context); // ✅ إضافة هنا
            ProgressRepository = new ProgressRepository(context);
            Genders = new Repository<Gender>(_context);
        }

        public IUserRepository Users { get; }
        public IRepository<Role> Roles { get; }
        public ICourseRepository Courses { get; }
        public IEnrollmentRepository Enrollments { get; }
        public ILessonRepository LessonRepository { get; }
        public ILessonCompletionRepository LessonCompletions { get; } // ✅
        public IProgressRepository ProgressRepository { get; }
        public IRepository<Gender> Genders { get; }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
