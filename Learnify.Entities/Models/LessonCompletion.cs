using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Entities.Models
{
    public class LessonCompletion
    {
        [Key]
        public int LessonCompletionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public Lesson? Lesson { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
