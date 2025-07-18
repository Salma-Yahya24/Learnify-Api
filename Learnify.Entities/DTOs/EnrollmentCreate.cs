using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Entities.DTOs;

public class EnrollmentCreate
{
    public int UserId { get; set; }
    public int CourseId { get; set; }
}
