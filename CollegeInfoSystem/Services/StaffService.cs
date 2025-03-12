using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Services;

public class StaffService
{
    private readonly CollegeDbContext _context;

    public StaffService(CollegeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Staff>> GetAllStaffAsync()
    {
        return await _context.Staff.ToListAsync();
    }

    public async Task<Staff> GetStaffByIdAsync(int id)
    {
        return await _context.Staff.FindAsync(id);
    }

    public async Task AddStaffAsync(Staff staff)
    {
        _context.Staff.Add(staff);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStaffAsync(Staff staff)
    {
        _context.Staff.Update(staff);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteStaffAsync(int id)
    {
        var staff = await _context.Staff.FindAsync(id);
        if (staff != null)
        {
            _context.Staff.Remove(staff);
            await _context.SaveChangesAsync();
        }
    }
}
