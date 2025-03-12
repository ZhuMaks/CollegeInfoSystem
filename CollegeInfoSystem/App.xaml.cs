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
            var studentViewModel = new StudentViewModel(studentService);

            var teacherService = new TeacherService(dbContext);
            var teacherViewModel = new TeacherViewModel(teacherService);

            var groupService = new GroupService(dbContext);
            var groupViewModel = new GroupViewModel(groupService);

            var scheduleService = new ScheduleService(dbContext);
            var scheduleViewModel = new ScheduleViewModel(scheduleService);

            var facultyService = new FacultyService(dbContext);
            var facultyViewModel = new FacultyViewModel(facultyService);

            var staffService = new StaffService(dbContext);
            var staffViewModel = new StaffViewModel(staffService);

            var mainViewModel = new MainViewModel(studentViewModel, teacherViewModel, groupViewModel, scheduleViewModel, facultyViewModel, staffViewModel);
            var mainWindow = new MainWindow { DataContext = mainViewModel };

            mainWindow.Show();
        }
    }
}
