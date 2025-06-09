using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.Services;

public class StudentService
{
    public async Task<List<Student>> GetAllStudentsAsync()
    {
        using var context = new CollegeDbContext();
        return await context.Students.Include(s => s.Group).ToListAsync();
    }

    public async Task<Student> GetStudentByIdAsync(int id)
    {
        using var context = new CollegeDbContext();
        return await context.Students.Include(s => s.Group).FirstOrDefaultAsync(s => s.StudentID == id);
    }

    public async Task AddStudentAsync(Student student)
    {
        using var context = new CollegeDbContext();
        context.Students.Add(student);
        await context.SaveChangesAsync();
    }

    public async Task UpdateStudentAsync(Student student)
    {
        using var context = new CollegeDbContext();

        var existingStudent = await context.Students.FindAsync(student.StudentID);
        if (existingStudent != null)
        {
            existingStudent.FirstName = student.FirstName;
            existingStudent.LastName = student.LastName;
            existingStudent.Email = student.Email;
            existingStudent.Phone = student.Phone;
            existingStudent.DateOfBirth = student.DateOfBirth;
            existingStudent.Address = student.Address;
            existingStudent.GroupID = student.GroupID;

            await context.SaveChangesAsync();
        }
    }


    public async Task DeleteStudentAsync(int id)
    {
        using var context = new CollegeDbContext();
        var student = await context.Students.FindAsync(id);
        if (student != null)
        {
            context.Students.Remove(student);
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
