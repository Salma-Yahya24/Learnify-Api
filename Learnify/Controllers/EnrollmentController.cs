using Learnify.Entities.DTOs;
using Learnify.Entities.Interfaces;
using Learnify.Entities.Models;
using Learnify.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Learnify.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public EnrollmentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet]
        public async Task<IActionResult> GetAllEnrollments()
        {
            var enrollments = await _unitOfWork.Enrollments.GetAllWithDetailsAsync();

            var enrollmentsDto = enrollments.Select(e => new EnrollmentRead
            {
                EnrollmentId = e.EnrollmentId,
                EnrolledAt = e.EnrolledAt,
                IsCompleted = e.IsCompleted,
                UserId = e.UserId,
                CourseId = e.CourseId,
                User = e.User?.UserName,
                Course = e.Course?.Title
            }).ToList();

            return Ok(enrollmentsDto);
        }

        [Authorize(Roles = "Admin,Instructor,Student")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnrollmentById(int id)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdWithDetailsAsync(id);

            if (enrollment == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Student"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                var userId = int.Parse(userIdClaim);
                if (enrollment.UserId != userId)
                {
                    return Forbid();
                }
            }

            var enrollmentDto = new EnrollmentRead
            {
                EnrollmentId = enrollment.EnrollmentId,
                EnrolledAt = enrollment.EnrolledAt,
                IsCompleted = enrollment.IsCompleted,
                UserId = enrollment.UserId,
                CourseId = enrollment.CourseId,
                User = enrollment.User?.UserName,
                Course = enrollment.Course?.Title
            };

            return Ok(enrollmentDto);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> CreateEnrollment([FromBody] EnrollmentCreate enrollmentCreate)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User ID is missing from the token." });
            }

            var userId = int.Parse(userIdClaim);

            // تحقق إذا كان المستخدم قد سجل بالفعل في الكورس
            var enrollments = await _unitOfWork.Enrollments.GetAllWithDetailsAsync();
            var existingEnrollment = enrollments
                .FirstOrDefault(e => e.UserId == userId && e.CourseId == enrollmentCreate.CourseId);

            if (existingEnrollment != null)
            {
                return BadRequest(new { message = "You are already enrolled in this course." });
            }

            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = enrollmentCreate.CourseId,
                EnrolledAt = DateTime.UtcNow,
                IsCompleted = false
            };

            await _unitOfWork.Enrollments.AddAsync(enrollment);
            await _unitOfWork.SaveAsync();

            // تحميل البيانات المرتبطة
            var createdEnrollment = await _unitOfWork.Enrollments.GetByIdWithDetailsAsync(enrollment.EnrollmentId);

            if (createdEnrollment == null)
            {
                return BadRequest("Failed to retrieve the newly created enrollment.");
            }

            var enrollmentDto = new EnrollmentRead
            {
                EnrollmentId = createdEnrollment.EnrollmentId,
                EnrolledAt = createdEnrollment.EnrolledAt,
                IsCompleted = createdEnrollment.IsCompleted,
                UserId = createdEnrollment.UserId,
                CourseId = createdEnrollment.CourseId,
                User = createdEnrollment.User?.UserName,
                Course = createdEnrollment.Course?.Title
            };

            return CreatedAtAction(nameof(GetEnrollmentById), new { id = createdEnrollment.EnrollmentId }, enrollmentDto);
        }

        // DELETE: api/enrollments/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(id);

            if (enrollment == null)
            {
                return NotFound();
            }

            _unitOfWork.Enrollments.Delete(enrollment);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        // DELETE: api/enrollments/cancel/{id}
        [Authorize(Roles = "Student")]
        [HttpDelete("cancel/{id}")]
        public async Task<IActionResult> CancelEnrollment(int id)
        {
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(id);

            if (enrollment == null)
            {
                return NotFound();
            }

            // Ensure that the student can only cancel their own enrollment
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (currentUserId != enrollment.UserId)
            {
                return Unauthorized(); // Student can only cancel their own enrollment
            }

            _unitOfWork.Enrollments.Delete(enrollment);
            await _unitOfWork.SaveAsync();

            return Ok(new { message = "Enrollment successfully canceled." });
        }
    }
}
