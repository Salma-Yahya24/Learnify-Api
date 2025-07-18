using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Entities.DTOs;

public class CreateCourse
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string Price { get; set; }

    // 👇 اسم الإنستراكتور بدل الـ ID
    public string? InstructorUserName { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }
}
