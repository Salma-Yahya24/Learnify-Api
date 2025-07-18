using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Entities.DTOs;

public class EnrollmentRead
{
    public int EnrollmentId { get; set; }
    public int UserId { get; set; }
    public string? User { get; set; } // Student name
    public int CourseId { get; set; }
    public string? Course { get; set; } // Course title
    public DateTime EnrolledAt { get; set; }
    public bool IsCompleted { get; set; }
}
