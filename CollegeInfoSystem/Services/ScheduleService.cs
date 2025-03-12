using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Services;

public class ScheduleService
{
    private readonly CollegeDbContext _context;

    public ScheduleService(CollegeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Schedule>> GetAllSchedulesAsync()
    {
        return await _context.Schedules
            .Include(s => s.Group)
            .Include(s => s.Teacher)
            .ToListAsync();
    }

    public async Task<List<Schedule>> GetSchedulesByGroupAsync(int groupId)
    {
        return await _context.Schedules
            .Where(s => s.GroupID == groupId)
            .Include(s => s.Teacher)
            .ToListAsync();
    }

    public async Task AddScheduleAsync(Schedule schedule)
    {
        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateScheduleAsync(Schedule schedule)
    {
        _context.Schedules.Update(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteScheduleAsync(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule != null)
        {
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }
    }
}
