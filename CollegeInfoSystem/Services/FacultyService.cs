using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Services;

public class FacultyService
{
    private readonly CollegeDbContext _context;

    public FacultyService(CollegeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Faculty>> GetAllFacultiesAsync()
    {
        return await _context.Faculties.ToListAsync();
    }

    public async Task<Faculty> GetFacultyByIdAsync(int id)
    {
        return await _context.Faculties.FindAsync(id);
    }

    public async Task AddFacultyAsync(Faculty faculty)
    {
        _context.Faculties.Add(faculty);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateFacultyAsync(Faculty faculty)
    {
        _context.Faculties.Update(faculty);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFacultyAsync(int id)
    {
        var faculty = await _context.Faculties.FindAsync(id);
        if (faculty != null)
        {
            _context.Faculties.Remove(faculty);
            await _context.SaveChangesAsync();
        }
    }
}
