using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Services;

public class StaffService
{
    public async Task<List<Staff>> GetAllStaffAsync()
    {
        using var context = new CollegeDbContext();
        return await context.Staff.ToListAsync();
    }

    public async Task<Staff> GetStaffByIdAsync(int id)
    {
        using var context = new CollegeDbContext();
        return await context.Staff.FindAsync(id);
    }

    public async Task AddStaffAsync(Staff staff)
    {
        using var context = new CollegeDbContext();
        context.Staff.Add(staff);
        await context.SaveChangesAsync();
    }

    public async Task UpdateStaffAsync(Staff staff)
    {
        using var context = new CollegeDbContext();
        context.Staff.Update(staff);
        await context.SaveChangesAsync();
    }

    public async Task DeleteStaffAsync(int id)
    {
        using var context = new CollegeDbContext();
        var staff = await context.Staff.FindAsync(id);
        if (staff != null)
        {
            context.Staff.Remove(staff);
            await context.SaveChangesAsync();
        }
    }
}
