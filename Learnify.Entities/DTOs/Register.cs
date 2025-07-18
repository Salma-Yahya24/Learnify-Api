using System.ComponentModel.DataAnnotations;


namespace Learnify.Entities.DTOs
{
    public class Register
    {
        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Password { get; set; } = string.Empty;

        [StringLength(15)]
        public string? TelephoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required]
        public string GenderName { get; set; } = string.Empty;

        [Required]
        public string RoleName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; } // إضافة حقل لصورة الملف الشخصي
    }
}