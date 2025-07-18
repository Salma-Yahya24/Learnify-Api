using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
namespace Learnify.Entities.DTOs;

public class CreateLesson
{
    public int CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public int Order { get; set; }

    // خاصية لرفع الملفات (فيديو أو PDF)
    public IFormFile? File { get; set; }

    // خاصية لرفع رابط الفيديو من يوتيوب أو رابط خارجي
    public string? VideoUrl { get; set; }

    // خاصية لرفع رابط PDF خارجي
    public string? PdfUrl { get; set; }
}

