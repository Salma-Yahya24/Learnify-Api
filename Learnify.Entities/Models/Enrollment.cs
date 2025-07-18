using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnify.Entities.Models;

public class Enrollment
{
    [Key]
    public int EnrollmentId { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }

    [Required]
    public int CourseId { get; set; }

    [ForeignKey("CourseId")]
    public Course? Course { get; set; }

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    [Required]
    public bool IsCompleted { get; set; } = false;

    public ICollection<Progress>? Progresses { get; set; } // ربط التقدم بالتسجيل
}
