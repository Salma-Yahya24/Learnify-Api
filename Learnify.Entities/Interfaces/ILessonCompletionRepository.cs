using Learnify.Entities.Models;
using Learnify.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Entities.Interfaces
{
    public interface ILessonCompletionRepository : IRepository<LessonCompletion>
    {
        // جلب جميع الدروس المكتملة لمستخدم معين في كورس معين
        Task<IEnumerable<LessonCompletion>> GetCompletedLessonsByUserAndCourseAsync(int userId, int courseId);

        // التحقق هل درس معين مكتمل للمستخدم
        Task<bool> IsLessonCompletedAsync(int userId, int lessonId);
    }
}
