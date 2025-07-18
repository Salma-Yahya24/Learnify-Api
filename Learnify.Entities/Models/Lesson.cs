using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Entities.Models
{
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        public int Order { get; set; }

        // رابط الفيديو الذي تم رفعه أو من يوتيوب
        public string? VideoUrl { get; set; }

        // رابط PDF إذا كان موجود
        public string? PdfUrl { get; set; }

        // مسار الفيديو أو PDF المرفوع
        public string? FilePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
