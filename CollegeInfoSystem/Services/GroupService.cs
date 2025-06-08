using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Services;

public class GroupService
{
    public async Task<List<Group>> GetAllGroupsAsync()
    {
        using var context = new CollegeDbContext();
        return await context.Groups
            .Include(g => g.Faculty)
            .Include(g => g.Curator)
            .ToListAsync();
    }

    public async Task<Group> GetGroupByIdAsync(int id)
    {
        using var context = new CollegeDbContext();
        return await context.Groups
            .Include(g => g.Faculty)
            .Include(g => g.Curator)
            .FirstOrDefaultAsync(g => g.GroupID == id);
    }

    public async Task AddGroupAsync(Group group)
    {
        using var context = new CollegeDbContext();

        if (group.Faculty != null)
        {
            context.Faculties.Attach(group.Faculty);
        }
        if (group.Curator != null)
        {
            context.Teachers.Attach(group.Curator);
        }

        context.Groups.Add(group);
        await context.SaveChangesAsync();
    }


    public async Task UpdateGroupAsync(Group group)
    {
        using var context = new CollegeDbContext();

        context.Entry(group).State = EntityState.Modified;

        if (group.Faculty != null)
        {
            context.Entry(group.Faculty).State = EntityState.Unchanged;
        }

        if (group.Curator != null)
        {
            context.Entry(group.Curator).State = EntityState.Unchanged;
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteGroupAsync(int id)
    {
        using var context = new CollegeDbContext();
        var group = await context.Groups.FindAsync(id);
        if (group != null)
        {
            context.Groups.Remove(group);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Student>> GetStudentsByGroupAsync(int groupId)
    {
        using var context = new CollegeDbContext();
        return await context.Students
            .Where(s => s.GroupID == groupId)
            .Include(s => s.Group)
            .ToListAsync();
    }
}
