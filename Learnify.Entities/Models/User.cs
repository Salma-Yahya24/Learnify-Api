using Learnify.Entities.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    [StringLength(15)]
    public string? TelephoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    // ربط مع Gender
    [ForeignKey("Gender")]
    public int GenderId { get; set; }  // الـ GenderId كـ Foreign Key
    public Gender Gender { get; set; }

    // ربط مع Role
    [ForeignKey("Role")]
    public int RoleId { get; set; }  // الـ RoleId كـ Foreign Key
    public Role Role { get; set; }

    public string? ResetCode { get; set; }
    public DateTime? ResetCodeExpiration { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    // ✅ إضافة مسار صورة الملف الشخصي
    [StringLength(300)]
    public string? ImageUrl { get; set; }
}
