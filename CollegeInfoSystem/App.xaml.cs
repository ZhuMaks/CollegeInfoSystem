using System.Windows;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;

namespace CollegeInfoSystem
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dbContext = new CollegeDbContext();

            var studentService = new StudentService(dbContext);
            var teacherService = new TeacherService(dbContext);
            var facultyService = new FacultyService(dbContext);
            var groupService = new GroupService(dbContext);
            var scheduleService = new ScheduleService(dbContext);
            var staffService = new StaffService(dbContext);

            var studentViewModel = new StudentViewModel(studentService, groupService);
            var teacherViewModel = new TeacherViewModel(teacherService);
            var facultyViewModel = new FacultyViewModel(facultyService);
            var staffViewModel = new StaffViewModel(staffService);
            var scheduleViewModel = new ScheduleViewModel(scheduleService);

            var groupViewModel = new GroupViewModel(groupService, facultyService, teacherService, studentService);

            var mainViewModel = new MainViewModel(studentViewModel, teacherViewModel, groupViewModel, scheduleViewModel, facultyViewModel, staffViewModel);
            var mainWindow = new MainWindow { DataContext = mainViewModel };

            mainWindow.Show();
        }

    }
}
