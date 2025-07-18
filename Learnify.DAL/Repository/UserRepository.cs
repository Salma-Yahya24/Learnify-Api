using Learnify.DAL.Data;
using Learnify.Entities.Interfaces;
using Learnify.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Learnify.Repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // ✅ دلوقتي تقدر تستخدم FindAsync بدون إضافة حاجة هنا
    }
}
