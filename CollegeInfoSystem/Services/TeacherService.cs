using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Services;

public class TeacherService
{
    public async Task<List<Teacher>> GetAllTeachersAsync()
    {
        using var context = new CollegeDbContext();
        return await context.Teachers.ToListAsync();
    }

    public async Task<Teacher> GetTeacherByIdAsync(int id)
    {
        using var context = new CollegeDbContext();
        return await context.Teachers.FirstOrDefaultAsync(t => t.TeacherID == id);
    }

    public async Task AddTeacherAsync(Teacher teacher)
    {
        using var context = new CollegeDbContext();
        context.Teachers.Add(teacher);
        await context.SaveChangesAsync();
    }

    public async Task UpdateTeacherAsync(Teacher teacher)
    {
        using var context = new CollegeDbContext();
        context.Teachers.Update(teacher);
        await context.SaveChangesAsync();
    }

    public async Task DeleteTeacherAsync(int id)
    {
        using var context = new CollegeDbContext();
        var teacher = await context.Teachers.FindAsync(id);
        if (teacher != null)
        {
            context.Teachers.Remove(teacher);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Teacher>> GetCuratorsAsync()
    {
        using var context = new CollegeDbContext();
        return await context.Teachers.Where(t => t.IsCurator).ToListAsync();
    }
}
