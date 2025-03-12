using CollegeInfoSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CollegeInfoSystem.Services;

public class CollegeDbContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Staff> Staff { get; set; }
    public DbSet<Faculty> Faculties { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Schedule> Schedules { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=CollegeDB;Trusted_Connection=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>()
            .HasOne(g => g.Curator)
            .WithMany()
            .HasForeignKey(g => g.CuratorID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Student>()
            .HasOne(s => s.Group)
            .WithMany(g => g.Students)
            .HasForeignKey(s => s.GroupID);

        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.Group)
            .WithMany()
            .HasForeignKey(s => s.GroupID);

        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.Teacher)
            .WithMany()
            .HasForeignKey(s => s.TeacherID);
    }
}
