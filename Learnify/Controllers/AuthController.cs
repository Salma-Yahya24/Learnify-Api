using Learnify.Entities.DTOs;
using Learnify.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using Learnify.Entities.Models;

namespace Learnify.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;

        public AuthController(IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettings)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] Register dto, [FromForm] IFormFile? profileImage)
        {
            // تحقق من وجود البريد الإلكتروني مسبقاً
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email already exists." });

            // الحصول على الدور حسب الاسم (RoleName)
            var role = (await _unitOfWork.Roles.GetAllAsync())
                .FirstOrDefault(r => string.Equals(r.RoleName, dto.RoleName, StringComparison.OrdinalIgnoreCase));

            if (role == null)
                return BadRequest("Invalid role name.");

            // الحصول على النوع (Gender) حسب الاسم (GenderName)
            var gender = (await _unitOfWork.Genders.GetAllAsync())
                .FirstOrDefault(g => string.Equals(g.GenderName, dto.GenderName, StringComparison.OrdinalIgnoreCase));

            if (gender == null)
                return BadRequest("Invalid gender name.");

            // تشفير كلمة المرور
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // حفظ صورة الملف الشخصي (إن وجدت)
            string? imageUrl = null;
            if (profileImage != null && profileImage.Length > 0)
            {
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(profileImage.FileName)}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profile-images", uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await profileImage.CopyToAsync(stream);

                imageUrl = $"/profile-images/{uniqueFileName}";
            }

            // إنشاء كائن مستخدم جديد مع تعيين RoleId بناءً على الدور المختار
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Password = hashedPassword,
                TelephoneNumber = dto.TelephoneNumber,
                DateOfBirth = dto.DateOfBirth,
                GenderId = gender.GenderId,
                RoleId = role.RoleId,
                ImageUrl = imageUrl
            };

            // إضافة المستخدم إلى قاعدة البيانات وحفظ التغييرات
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();

            return Ok(new { message = "User registered successfully." });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login dto)
        {
            var matchedUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
            if (matchedUser == null || !BCrypt.Net.BCrypt.Verify(dto.Password, matchedUser.Password))
                return Unauthorized("Invalid email or password.");

            var token = await GenerateJwtToken(matchedUser);

            return Ok(new
            {
                Token = token,
                UserId = matchedUser.UserId,
                UserName = matchedUser.UserName,
                Email = matchedUser.Email,
                Role = (await _unitOfWork.Roles.GetByIdAsync(matchedUser.RoleId))?.RoleName,
                ImageUrl = matchedUser.ImageUrl
            });
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var role = (await _unitOfWork.Roles.GetByIdAsync(user.RoleId))?.RoleName;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
