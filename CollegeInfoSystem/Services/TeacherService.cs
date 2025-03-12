using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Services;

public class TeacherService
{
    private readonly CollegeDbContext _context;

    public TeacherService(CollegeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Teacher>> GetAllTeachersAsync()
    {
        return await _context.Teachers.ToListAsync();
    }

    public async Task<Teacher> GetTeacherByIdAsync(int id)
    {
        return await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherID == id);
    }

    public async Task AddTeacherAsync(Teacher teacher)
    {
        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTeacherAsync(Teacher teacher)
    {
        _context.Teachers.Update(teacher);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTeacherAsync(int id)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher != null)
        {
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Teacher>> GetCuratorsAsync()
    {
        return await _context.Teachers.Where(t => t.IsCurator).ToListAsync();
    }
}
