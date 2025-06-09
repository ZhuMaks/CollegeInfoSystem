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

        schedule.Group = null;
        schedule.Teacher = null;

        context.Schedules.Add(schedule);
        await context.SaveChangesAsync();
    }


    public async Task UpdateScheduleAsync(Schedule schedule)
    {
        using var context = new CollegeDbContext();

        var existing = await context.Schedules.FindAsync(schedule.ScheduleID);
        if (existing != null)
        {
            existing.Subject = schedule.Subject;
            existing.DayOfWeek = schedule.DayOfWeek;
            existing.StartTime = schedule.StartTime;
            existing.EndTime = schedule.EndTime;
            existing.Room = schedule.Room;
            existing.GroupID = schedule.GroupID;
            existing.TeacherID = schedule.TeacherID;

            await context.SaveChangesAsync();
        }
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
