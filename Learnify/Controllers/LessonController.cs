using Learnify.Entities.DTOs;
using Learnify.Entities.Models;
using Learnify.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Learnify.API.Controllers;

[Route("api/lessons")]
[ApiController]
public class LessonsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public LessonsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private IActionResult CreateErrorResponse(string message)
    {
        return BadRequest(new { message });
    }

    // GET: api/lessons/course/{courseId}
    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetLessonsByCourse(int courseId)
    {
        // جلب جميع الدروس الخاصة بالدورة التدريبية
        var lessons = await _unitOfWork.LessonRepository.GetLessonsByCourseIdAsync(courseId);

        // التحقق من وجود الكورس
        var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
        if (course == null)
            return NotFound(new { message = "Course not found." });

        // دمج معلومات الكورس مع كل درس
        var lessonDetails = lessons.Select(lesson => new
        {
            lesson.LessonId,
            lesson.CourseId,
            lesson.Title,
            lesson.Content,
            lesson.Order,
            CourseName = course.Title, // إضافة اسم الكورس
            lesson.VideoUrl,           // إضافة رابط الفيديو
            lesson.PdfUrl              // إضافة رابط PDF
        }).ToList();

        return Ok(lessonDetails);
    }

    // GET: api/lessons/{courseId}/{lessonId}
    [HttpGet("{courseId:int}/{lessonId:int}")]
    [Authorize]
    public async Task<IActionResult> GetLesson(int courseId, int lessonId)
    {
        var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(lessonId);
        if (lesson == null || lesson.CourseId != courseId)
            return NotFound(new { message = "Lesson not found for this course." });

        // تحميل معلومات الكورس المرتبطة بالدرس
        var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
        if (course == null)
            return NotFound(new { message = "Course not found." });

        if (User.IsInRole("Student"))
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "User ID not found in token." });

            var userId = int.Parse(userIdClaim.Value);

            var enrollment = await _unitOfWork.Enrollments.GetByUserAndCourseAsync(userId, courseId);

            if (enrollment == null)
                return Forbid("You must enroll in this course to access its lessons.");
        }

        // إعادة الدرس مع اسم الكورس
        return Ok(new
        {
            lesson.LessonId,
            lesson.CourseId,
            lesson.Title,
            lesson.Content,
            lesson.Order,
            CourseName = course.Title, // اسم الكورس
            lesson.VideoUrl,           // رابط الفيديو
            lesson.PdfUrl              // رابط PDF
        });
    }

    [HttpPost]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> CreateLesson([FromForm] CreateLesson dto, IFormFile? file)
    {
        var lessons = await _unitOfWork.LessonRepository.GetAllAsync();
        var existingLesson = lessons.FirstOrDefault(l => l.CourseId == dto.CourseId && l.Title == dto.Title);

        if (existingLesson != null)
        {
            return CreateErrorResponse($"A lesson with the title '{dto.Title}' already exists in this course.");
        }

        // تحقق إن الـ Instructor هو صاحب الكورس
        var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId);
        if (course == null)
            return NotFound(new { message = "Course not found." });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized(new { message = "User ID not found in token." });

        var userId = int.Parse(userIdClaim.Value);
        if (User.IsInRole("Instructor") && course.InstructorId != userId)
        {
            // رسالة خطأ مخصصة عند محاولة إضافة درس في كورس ليس ملكًا له
            return BadRequest(new { message = "You can only add lessons to your own courses." });
        }

        string? fileUrl = null;

        if (file != null)
        {
            var allowedExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv", ".pdf" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file format. Allowed formats are .mp4, .avi, .mov, .mkv, .pdf." });
            }

            var folderName = "lesson-file";
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = $"{dto.CourseId}_{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // إنشاء الرابط الكامل
            fileUrl = $"{Request.Scheme}://{Request.Host}/{folderName}/{fileName}";
        }

        var lesson = new Lesson
        {
            CourseId = dto.CourseId,
            Title = dto.Title,
            Content = dto.Content,
            Order = dto.Order,
            VideoUrl = dto.VideoUrl,
            PdfUrl = dto.PdfUrl,
            FilePath = fileUrl
        };

        await _unitOfWork.LessonRepository.AddAsync(lesson);
        await _unitOfWork.SaveAsync();

        return CreatedAtAction(nameof(GetLesson), new { courseId = lesson.CourseId, lessonId = lesson.LessonId }, new
        {
            lesson.LessonId,
            lesson.CourseId,
            lesson.Title,
            lesson.Content,
            lesson.Order,
            lesson.VideoUrl,
            lesson.PdfUrl,
            lesson.FilePath,
            CourseName = course.Title
        });
    }
    // POST: api/lessons/{courseId}/{lessonId}/complete
    [HttpPost("{courseId:int}/{lessonId:int}/complete")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CompleteLesson(int courseId, int lessonId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized(new { message = "User ID not found in token." });

        int userId = int.Parse(userIdClaim.Value);

        // تحقق إن الدرس موجود وينتمي للكورس
        var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(lessonId);
        if (lesson == null || lesson.CourseId != courseId)
            return NotFound(new { message = "Lesson not found for this course." });

        // تحقق إن المستخدم مسجل في الكورس
        var enrollment = await _unitOfWork.Enrollments.GetByUserAndCourseAsync(userId, courseId);
        if (enrollment == null)
            return Forbid("You must enroll in this course to mark lessons as completed.");

        // تحقق إن الدرس لم يتم إكماله مسبقًا
        var existingCompletion = await _unitOfWork.LessonCompletions.FindAsync(
            lc => lc.UserId == userId && lc.LessonId == lessonId);

        if (existingCompletion != null)
            return BadRequest(new { message = "Lesson already marked as completed." });

        // تسجيل إكمال الدرس
        var lessonCompletion = new LessonCompletion
        {
            UserId = userId,
            LessonId = lessonId,
            CompletedAt = DateTime.UtcNow
        };

        await _unitOfWork.LessonCompletions.AddAsync(lessonCompletion);

        // تحديث التقدم تلقائيًا (نفترض عندك دالة UpdateProgressAsync)
        try
        {
            await UpdateProgressAsync(userId, courseId);
        }
        catch (Exception ex)
        {
            // ممكن تسجل الخطأ لكن لا تمنع إكمال الدرس
        }

        await _unitOfWork.SaveAsync();

        return Ok(new { message = "Lesson marked as completed." });
    }

    // PUT: api/lessons/{courseId}/{lessonId}
    [HttpPut("{courseId:int}/{lessonId:int}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> UpdateLesson(int courseId, int lessonId, [FromForm] UpdateLesson dto, IFormFile? file)
    {
        var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(lessonId);
        if (lesson == null || lesson.CourseId != courseId)
            return NotFound(new { message = "Lesson not found for this course." });

        // تحقق إن الـ Instructor هو صاحب الكورس
        var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
        if (course == null)
            return NotFound(new { message = "Course not found." });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized(new { message = "User ID not found in token." });

        var userId = int.Parse(userIdClaim.Value);
        if (User.IsInRole("Instructor") && course.InstructorId != userId)
        {
            return Forbid("You can only modify your own courses.");
        }

        lesson.Title = dto.Title;
        lesson.Content = dto.Content;
        lesson.Order = dto.Order;

        // إذا تم إرسال ملف جديد
        if (file != null)
        {
            var allowedExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv", ".pdf" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file format. Allowed formats are .mp4, .avi, .mov, .mkv, .pdf." });
            }

            var folderName = "lesson-file";
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = $"{lessonId}_{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadPath, fileName);

            // حذف الملف القديم إذا كان موجود
            if (!string.IsNullOrEmpty(lesson.FilePath))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", lesson.FilePath.TrimStart('/').Replace("/", "\\"));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            lesson.FilePath = Path.Combine("/", folderName, fileName).Replace("\\", "/");
        }

        lesson.VideoUrl = dto.VideoUrl;
        lesson.PdfUrl = dto.PdfUrl;

        _unitOfWork.LessonRepository.Update(lesson);
        await _unitOfWork.SaveAsync();

        return NoContent();
    }

    // DELETE: api/lessons/{courseId}/{lessonId}
    [HttpDelete("{courseId:int}/{lessonId:int}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> DeleteLesson(int courseId, int lessonId)
    {
        var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(lessonId);
        if (lesson == null || lesson.CourseId != courseId)
            return NotFound(new { message = "Lesson not found for this course." });

        // تحقق إن الـ Instructor هو صاحب الكورس
        var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
        if (course == null)
            return NotFound(new { message = "Course not found." });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized(new { message = "User ID not found in token." });

        var userId = int.Parse(userIdClaim.Value);
        if (User.IsInRole("Instructor") && course.InstructorId != userId)
        {
            return Forbid("You can only delete lessons from your own courses.");
        }

        // حذف الملف الفعلي من السيرفر
        if (!string.IsNullOrEmpty(lesson.FilePath))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", lesson.FilePath.TrimStart('/').Replace("/", "\\"));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        _unitOfWork.LessonRepository.Delete(lesson);
        await _unitOfWork.SaveAsync();

        return NoContent();
    }
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
