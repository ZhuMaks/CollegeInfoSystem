using System.Windows;
using CollegeInfoSystem.Views;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;

namespace CollegeInfoSystem
{
    public partial class App : Application
    {
        public static string CurrentUserRole { get; set; } = string.Empty;
        public static string CurrentTheme { get; set; } = "light";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var loginWindow = new LoginWindow();
            var loginViewModel = new LoginViewModel();

            loginViewModel.LoginSucceeded += role =>
            {
                CurrentUserRole = role;

                var dbContext = new CollegeDbContext();

                var studentService = new StudentService(dbContext);
                var teacherService = new TeacherService(dbContext);
                var facultyService = new FacultyService(dbContext);
                var groupService = new GroupService(dbContext);
                var scheduleService = new ScheduleService(dbContext);
                var staffService = new StaffService(dbContext);

                var studentViewModel = new StudentViewModel(studentService, groupService, CurrentUserRole);
                var teacherViewModel = new TeacherViewModel(teacherService, CurrentUserRole);
                var facultyViewModel = new FacultyViewModel(facultyService, CurrentUserRole);
                var staffViewModel = new StaffViewModel(staffService, CurrentUserRole);
                var scheduleViewModel = new ScheduleViewModel(scheduleService, groupService, teacherService, CurrentUserRole);
                var groupViewModel = new GroupViewModel(groupService, facultyService, teacherService, studentService, CurrentUserRole);

                var mainViewModel = new MainViewModel(CurrentUserRole, studentViewModel, teacherViewModel, groupViewModel, scheduleViewModel, facultyViewModel, staffViewModel);
                var mainWindow = new MainWindow(CurrentUserRole)
                {
                    DataContext = mainViewModel
                };

                loginWindow.Close(); 
                mainWindow.Show();   
            };

            loginWindow.DataContext = loginViewModel;
            loginWindow.ShowDialog();

            if (string.IsNullOrEmpty(CurrentUserRole))
            {
                Shutdown(); 
            }
        }
    }
}
