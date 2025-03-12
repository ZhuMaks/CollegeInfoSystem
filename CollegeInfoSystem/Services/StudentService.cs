using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Services;

public class StudentService
{
    private readonly CollegeDbContext _context;

    public StudentService(CollegeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Student>> GetAllStudentsAsync()
    {
        return await _context.Students.Include(s => s.Group).ToListAsync();
    }

    public async Task<Student> GetStudentByIdAsync(int id)
    {
        return await _context.Students.Include(s => s.Group).FirstOrDefaultAsync(s => s.StudentID == id);
    }

    public async Task AddStudentAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStudentAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteStudentAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student != null)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Student>> GetStudentsByGroupAsync(int groupId)
    {
        return await _context.Students.Where(s => s.GroupID == groupId).ToListAsync();
    }
}
