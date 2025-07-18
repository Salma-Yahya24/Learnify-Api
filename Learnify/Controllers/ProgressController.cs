using Learnify.Entities.DTOs;
using Learnify.Entities.Models;
using Learnify.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Learnify.Controllers
{
    [ApiController]
    [Route("api/progress")]
    [Authorize]
    public class ProgressController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProgressController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ✅ 1. Get progress by user and course
        [HttpGet("user/{userId}/course/{courseId}")]
        public async Task<IActionResult> GetProgressByUserAndCourse(int userId, int courseId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (User.IsInRole("Student") && currentUserId != userId)
                return Forbid();

            // تحقق من وجود الاشتراك أولاً
            var enrollment = await _unitOfWork.Enrollments.GetByUserAndCourseAsync(userId, courseId);
            if (enrollment == null)
                return BadRequest("User is not enrolled in this course.");

            var progress = (await _unitOfWork.ProgressRepository.GetAllAsync())
                .FirstOrDefault(p => p.UserId == userId && p.CourseId == courseId);

            if (progress == null)
                return NotFound("Progress not found.");

            var dto = MapToDto(progress);
            return Ok(dto);
        }

        // ✅ 2. Get all progress for current user
        [HttpGet("my-progress")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyProgress()
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var progresses = (await _unitOfWork.ProgressRepository.GetAllAsync())
                .Where(p => p.UserId == currentUserId)
                .Select(MapToDto)
                .ToList();

            return Ok(progresses);
        }

        // ✅ 4. Update progress automatically based on completed lessons
        [HttpPost("auto-update/user/{userId}/course/{courseId}")]
        public async Task<IActionResult> AutoUpdateProgress(int userId, int courseId)
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserIdStr, out int currentUserId))
                return Unauthorized("Invalid token.");

            if (User.IsInRole("Student") && currentUserId != userId)
                return Forbid();

            try
            {
                var updatedProgress = await UpdateProgressAsync(userId, courseId);
                return Ok(updatedProgress);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ✅ 3. Update progress (by ID)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProgress(int id, [FromBody] double percentage)
        {
            var progress = await _unitOfWork.ProgressRepository.GetByIdAsync(id);
            if (progress == null)
                return NotFound("Progress not found.");

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // تحقق من الاشتراك في الكورس
            var enrollment = await _unitOfWork.Enrollments.GetByUserAndCourseAsync(progress.UserId, progress.CourseId);
            if (enrollment == null)
                return BadRequest("User is not enrolled in this course.");

            if (User.IsInRole("Student") && progress.UserId != currentUserId)
                return Forbid();

            progress.CompletionPercentage = Math.Clamp(percentage, 0, 100);
            progress.LastUpdated = DateTime.UtcNow;

            _unitOfWork.ProgressRepository.Update(progress);
            await _unitOfWork.SaveAsync();

            return Ok(MapToDto(progress));
        }

        // ✅ Helper method to map Progress to DTO
        private ProgressDto MapToDto(Progress progress)
        {
            return new ProgressDto
            {
                ProgressId = progress.ProgressId,
                UserId = progress.UserId,
                CourseId = progress.CourseId,
                EnrollmentId = progress.EnrollmentId,
                CompletionPercentage = progress.CompletionPercentage,
                LastUpdated = progress.LastUpdated
            };
        }

        // داخل ProgressController أو في خدمة مستقلة (مثلاً ProgressService)
        private async Task<ProgressDto> UpdateProgressAsync(int userId, int courseId)
        {
            // تحقق من وجود الاشتراك
            var enrollment = await _unitOfWork.Enrollments.GetByUserAndCourseAsync(userId, courseId);
            if (enrollment == null)
                throw new Exception("User is not enrolled in this course.");

            // جلب جميع دروس الكورس
            var lessons = await _unitOfWork.LessonRepository.GetLessonsByCourseIdAsync(courseId);
            int totalLessons = lessons.Count();

            if (totalLessons == 0)
                throw new Exception("Course has no lessons.");

            // جلب الدروس المكتملة للمستخدم في هذا الكورس
            var completedLessons = (await _unitOfWork.LessonCompletions.FindAllAsync(
                lc => lc.UserId == userId && lessons.Select(l => l.LessonId).Contains(lc.LessonId)));
            int completedCount = completedLessons.Count();

            // حساب النسبة المئوية
            double completionPercentage = (double)completedCount / totalLessons * 100;

            // جلب أو إنشاء كائن Progress
            var progress = (await _unitOfWork.ProgressRepository.FindAsync(p => p.UserId == userId && p.CourseId == courseId))
                            ?? new Progress
                            {
                                UserId = userId,
                                CourseId = courseId,
                                EnrollmentId = enrollment.EnrollmentId,
                                CompletionPercentage = 0,
                                LastUpdated = DateTime.UtcNow
                            };

            progress.CompletionPercentage = Math.Clamp(completionPercentage, 0, 100);
            progress.LastUpdated = DateTime.UtcNow;

            if (progress.ProgressId == 0)
                await _unitOfWork.ProgressRepository.AddAsync(progress);
            else
                _unitOfWork.ProgressRepository.Update(progress);

            await _unitOfWork.SaveAsync();

            return MapToDto(progress);
        }

    }
}
