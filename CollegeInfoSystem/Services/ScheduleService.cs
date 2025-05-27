using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Services;

public class ScheduleService
{
    public async Task<List<Schedule>> GetAllSchedulesAsync()
    {
        using var context = new CollegeDbContext();
        return await context.Schedules
            .Include(s => s.Group)
            .Include(s => s.Teacher)
            .ToListAsync();
    }

    public async Task<List<Schedule>> GetSchedulesByGroupAsync(int groupId)
    {
        using var context = new CollegeDbContext();
        return await context.Schedules
            .Where(s => s.GroupID == groupId)
            .Include(s => s.Teacher)
            .Include(s => s.Group)
            .ToListAsync();
    }

    public async Task AddScheduleAsync(Schedule schedule)
    {
        using var context = new CollegeDbContext();
        context.Schedules.Add(schedule);
        await context.SaveChangesAsync();
    }

    public async Task UpdateScheduleAsync(Schedule schedule)
    {
        using var context = new CollegeDbContext();
        context.Schedules.Update(schedule);
        await context.SaveChangesAsync();
    }

    public async Task DeleteScheduleAsync(int id)
    {
        using var context = new CollegeDbContext();
        var schedule = await context.Schedules.FindAsync(id);
        if (schedule != null)
        {
            context.Schedules.Remove(schedule);
            await context.SaveChangesAsync();
        }
    }
}
