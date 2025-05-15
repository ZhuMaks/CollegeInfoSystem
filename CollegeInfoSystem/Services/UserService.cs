using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CollegeInfoSystem.Services
{
    public class UserService
    {
        private readonly CollegeDbContext _context;

        public UserService(CollegeDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            string hash = HashPassword(password);
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hash);
        }

        public async Task<bool> RegisterAsync(string username, string password, string role)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
                return false;

            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
