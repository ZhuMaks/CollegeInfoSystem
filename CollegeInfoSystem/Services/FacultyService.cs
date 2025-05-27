using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Services;

public class FacultyService
{
    public async Task<List<Faculty>> GetAllFacultiesAsync()
    {
        using var context = new CollegeDbContext();
        return await context.Faculties.ToListAsync();
    }

    public async Task<Faculty> GetFacultyByIdAsync(int id)
    {
        using var context = new CollegeDbContext();
        return await context.Faculties.FindAsync(id);
    }

    public async Task AddFacultyAsync(Faculty faculty)
    {
        using var context = new CollegeDbContext();
        context.Faculties.Add(faculty);
        await context.SaveChangesAsync();
    }

    public async Task UpdateFacultyAsync(Faculty faculty)
    {
        using var context = new CollegeDbContext();
        context.Faculties.Update(faculty);
        await context.SaveChangesAsync();
    }

    public async Task DeleteFacultyAsync(int id)
    {
        using var context = new CollegeDbContext();
        var faculty = await context.Faculties.FindAsync(id);
        if (faculty != null)
        {
            context.Faculties.Remove(faculty);
            await context.SaveChangesAsync();
        }
    }
}
