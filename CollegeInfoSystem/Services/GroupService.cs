using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Services;

public class GroupService
{
    private readonly CollegeDbContext _context;

    public GroupService(CollegeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Group>> GetAllGroupsAsync()
    {
        return await _context.Groups.Include(g => g.Faculty).Include(g => g.Curator).ToListAsync();
    }

    public async Task<Group> GetGroupByIdAsync(int id)
    {
        return await _context.Groups.Include(g => g.Faculty).Include(g => g.Curator)
            .FirstOrDefaultAsync(g => g.GroupID == id);
    }

    public async Task AddGroupAsync(Group group)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateGroupAsync(Group group)
    {
        _context.Groups.Update(group);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteGroupAsync(int id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group != null)
        {
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Student>> GetStudentsByGroupAsync(int groupId)
    {
        return await _context.Students.Where(s => s.GroupID == groupId).ToListAsync();
    }
}
