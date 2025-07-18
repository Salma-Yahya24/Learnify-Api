
using Learnify.Entities.DTOs;
using Learnify.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Learnify.Entities.Models;

namespace Learnify.Controllers
{
    [Route("api/courses")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _courseImagesPath;

        public CourseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _courseImagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "course-images");

            if (!Directory.Exists(_courseImagesPath))
                Directory.CreateDirectory(_courseImagesPath);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _unitOfWork.Courses.GetAllWithInstructorAsync();


            var response = courses.Select(course => new CourseDetails
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                ImageUrl = course.ImageUrl,
                Instructor = new SimpleInstructor
                {
                    UserName = course.Instructor?.UserName ?? "N/A",
                    Email = course.Instructor?.Email ?? "N/A"
                }
            });

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _unitOfWork.Courses.GetByIdWithInstructorAsync(id);
            if (course == null)
                return NotFound();

            var response = new CourseDetails
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                ImageUrl = course.ImageUrl,
                Instructor = new SimpleInstructor
                {
                    UserName = course.Instructor?.UserName ?? "N/A",
                    Email = course.Instructor?.Email ?? "N/A"
                }
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CreateCourse([FromForm] CreateCourse createCourse, [FromForm] IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                return Unauthorized("Invalid or missing user ID in token.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return Unauthorized("User not found.");

            var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId);
            if (role == null)
                return Forbid("Invalid role.");

            int instructorId;

            if (role.RoleName == "Admin")
            {
                if (string.IsNullOrWhiteSpace(createCourse.InstructorUserName))
                    return BadRequest("InstructorUserName is required for admins.");

                var instructor = await _unitOfWork.Users
                    .FindAsync(u => u.UserName.ToLower() == createCourse.InstructorUserName.ToLower());

                if (instructor == null || (await _unitOfWork.Roles.GetByIdAsync(instructor.RoleId))?.RoleName != "Instructor")
                    return BadRequest("Instructor not found or not a valid instructor.");

                instructorId = instructor.UserId;
            }
            else if (role.RoleName == "Instructor")
            {
                instructorId = userId;
            }
            else
            {
                return Forbid("Only Admins and Instructors can create courses.");
            }

            if (!decimal.TryParse(createCourse.Price, out var parsedPrice))
                return BadRequest("Invalid price format.");

            string? imageUrl = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(_courseImagesPath, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                imageUrl = $"/course-images/{uniqueFileName}";
            }

            var course = new Course
            {
                Title = createCourse.Title,
                Description = createCourse.Description,
                Price = parsedPrice,
                InstructorId = instructorId,
                ImageUrl = imageUrl
            };

            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.SaveAsync();

            var instructorUser = await _unitOfWork.Users.GetByIdAsync(instructorId);

            var response = new CourseDetails
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                ImageUrl = course.ImageUrl,
                Instructor = new SimpleInstructor
                {
                    UserName = instructorUser?.UserName ?? "N/A",
                    Email = instructorUser?.Email ?? "N/A"
                }
            };

            return CreatedAtAction(nameof(GetCourseById), new { id = course.CourseId }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> UpdateCourse(int id, [FromForm] UpdateCourse updateCourse, [FromForm] IFormFile? imageFile)
        {
            var existingCourse = await _unitOfWork.Courses.GetByIdAsync(id);
            if (existingCourse == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId);

            if (role?.RoleName == "Instructor" && existingCourse.InstructorId != userId)
                return Forbid("Instructors can only update their own courses.");

            var updatedFields = new List<string>();

            if (!string.IsNullOrWhiteSpace(updateCourse.Title) && updateCourse.Title != existingCourse.Title)
            {
                existingCourse.Title = updateCourse.Title;
                updatedFields.Add("Title");
            }

            if (!string.IsNullOrWhiteSpace(updateCourse.Description) && updateCourse.Description != existingCourse.Description)
            {
                existingCourse.Description = updateCourse.Description;
                updatedFields.Add("Description");
            }

            if (!string.IsNullOrWhiteSpace(updateCourse.Price))
            {
                if (!decimal.TryParse(updateCourse.Price, out var parsedPrice))
                    return BadRequest("Invalid price format.");

                if (parsedPrice != existingCourse.Price)
                {
                    existingCourse.Price = parsedPrice;
                    updatedFields.Add("Price");
                }
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingCourse.ImageUrl))
                {
                    var oldFilePath = Path.Combine(_courseImagesPath, Path.GetFileName(existingCourse.ImageUrl));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var newFilePath = Path.Combine(_courseImagesPath, uniqueFileName);

                using var stream = new FileStream(newFilePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                existingCourse.ImageUrl = $"/course-images/{uniqueFileName}";
                updatedFields.Add("Image");
            }

            if (!updatedFields.Any())
                return BadRequest("No valid fields provided for update.");

            _unitOfWork.Courses.Update(existingCourse);
            await _unitOfWork.SaveAsync();

            return Ok(new
            {
                Message = "Course updated successfully.",
                UpdatedFields = updatedFields
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null)
                return NotFound();

            // حذف صورة الكورس إذا كانت موجودة
            if (!string.IsNullOrEmpty(course.ImageUrl))
            {
                var filePath = Path.Combine(_courseImagesPath, Path.GetFileName(course.ImageUrl));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            // حذف الكورس من قاعدة البيانات
            _unitOfWork.Courses.Delete(course);
            await _unitOfWork.SaveAsync();

            // إرسال رسالة تأكيد للمستخدم
            return Ok(new { Message = "Course deleted successfully." });
        }

    }
}