using Learnify.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Learnify.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Course> Courses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<LessonCompletion> LessonCompletions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تخصيص نوع العمود لـ Price في جدول Courses
            modelBuilder.Entity<Course>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            // تخصيص الطول الأقصى لعمود ImageUrl في Course و Lesson و User
            modelBuilder.Entity<Course>()
                .Property(c => c.ImageUrl)
                .HasMaxLength(300);

            modelBuilder.Entity<User>()
                .Property(u => u.ImageUrl)
                .HasMaxLength(300);

            // علاقة Enrollment -> User
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة Enrollment -> Course
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة User -> Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة User -> Gender
            modelBuilder.Entity<User>()
                .HasOne(u => u.Gender)
                .WithMany(g => g.Users)
                .HasForeignKey(u => u.GenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة Progress -> User
            modelBuilder.Entity<Progress>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة Progress -> Course
            modelBuilder.Entity<Progress>()
                .HasOne(p => p.Course)
                .WithMany()
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة Progress -> Enrollment
            modelBuilder.Entity<Progress>()
                .HasOne(p => p.Enrollment)
                .WithMany(e => e.Progresses)
                .HasForeignKey(p => p.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة LessonCompletion -> User
            modelBuilder.Entity<LessonCompletion>()
                .HasOne(lc => lc.User)
                .WithMany()
                .HasForeignKey(lc => lc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة LessonCompletion -> Lesson
            modelBuilder.Entity<LessonCompletion>()
                .HasOne(lc => lc.Lesson)
                .WithMany()
                .HasForeignKey(lc => lc.LessonId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
