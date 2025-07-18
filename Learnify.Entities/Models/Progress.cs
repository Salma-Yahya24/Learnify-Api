using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learnify.Entities.Models;

public class Progress
{
    [Key]
    public int ProgressId { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }

    [Required]
    public int CourseId { get; set; }

    [ForeignKey("CourseId")]
    public Course? Course { get; set; }

    [Required]
    public int EnrollmentId { get; set; }

    [ForeignKey("EnrollmentId")]
    public Enrollment? Enrollment { get; set; }

    [Range(0, 100)]
    public double CompletionPercentage { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
