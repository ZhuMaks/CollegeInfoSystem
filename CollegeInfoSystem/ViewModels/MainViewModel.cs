using System.Windows;
using System.Windows.Input;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CollegeInfoSystem.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object? _currentView;
        public object? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        private readonly string _userRole;

        private readonly StudentsView _studentsView;
        private readonly TeachersView _teachersView;
        private readonly GroupsView _groupsView;
        private readonly ScheduleView _scheduleView;
        private readonly FacultyView _facultyView;
        private readonly StaffView _staffView;
        private readonly UsersView _usersView;

        private readonly StudentViewModel _studentViewModel;
        private readonly TeacherViewModel _teacherViewModel;
        private readonly GroupViewModel _groupViewModel;
        private readonly ScheduleViewModel _scheduleViewModel;
        private readonly FacultyViewModel _facultyViewModel;
        private readonly StaffViewModel _staffViewModel;
        private readonly UsersViewModel _usersViewModel;

        public ICommand OpenStudentsViewCommand { get; }
        public ICommand OpenTeachersViewCommand { get; }
        public ICommand OpenGroupsViewCommand { get; }
        public ICommand OpenScheduleViewCommand { get; }
        public ICommand OpenFacultyViewCommand { get; }
        public ICommand OpenStaffViewCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand? OpenUsersViewCommand { get; }

        public string UserRole => _userRole;

        public MainViewModel(string userRole, StudentViewModel studentViewModel, TeacherViewModel teacherViewModel,
            GroupViewModel groupViewModel, ScheduleViewModel scheduleViewModel,
            FacultyViewModel facultyViewModel, StaffViewModel staffViewModel)
        {
            _userRole = userRole;

            _studentViewModel = studentViewModel;
            _teacherViewModel = teacherViewModel;
            _groupViewModel = groupViewModel;
            _scheduleViewModel = scheduleViewModel;
            _facultyViewModel = facultyViewModel;
            _staffViewModel = staffViewModel;

            _studentsView = new StudentsView { DataContext = _studentViewModel };
            _teachersView = new TeachersView { DataContext = _teacherViewModel };
            _groupsView = new GroupsView { DataContext = _groupViewModel };
            _scheduleView = new ScheduleView { DataContext = _scheduleViewModel };
            _facultyView = new FacultyView { DataContext = _facultyViewModel };
            _staffView = new StaffView { DataContext = _staffViewModel };

            OpenStudentsViewCommand = new RelayCommand(() => SetCurrentView(_studentsView, _studentViewModel));
            OpenTeachersViewCommand = new RelayCommand(() => SetCurrentView(_teachersView, _teacherViewModel));
            OpenGroupsViewCommand = new RelayCommand(() => SetCurrentView(_groupsView, _groupViewModel));
            OpenScheduleViewCommand = new RelayCommand(() => SetCurrentView(_scheduleView, _scheduleViewModel));
            OpenFacultyViewCommand = new RelayCommand(() => SetCurrentView(_facultyView, _facultyViewModel));
            OpenStaffViewCommand = new RelayCommand(() => SetCurrentView(_staffView, _staffViewModel));

            if (_userRole == "admin")
            {
                var dbContext = new CollegeDbContext();
                var userService = new UserService(dbContext);
                _usersViewModel = new UsersViewModel(userService, dbContext, _userRole);
                _usersView = new UsersView { DataContext = _usersViewModel };
                OpenUsersViewCommand = new RelayCommand(() => SetCurrentView(_usersView, _usersViewModel));
            }

            LogoutCommand = new RelayCommand(Logout);
        }

        private void Logout()
        {
            var loginWindow = new LoginWindow();
            var loginViewModel = new LoginViewModel();
            loginWindow.DataContext = loginViewModel;

            loginViewModel.LoginSucceeded += role =>
            {
                App.CurrentUserRole = role;

                var dbContext = new CollegeDbContext();
                var studentService = new StudentService(dbContext);
                var teacherService = new TeacherService(dbContext);
                var facultyService = new FacultyService(dbContext);
                var groupService = new GroupService(dbContext);
                var scheduleService = new ScheduleService(dbContext);
                var staffService = new StaffService(dbContext);

                var studentViewModel = new StudentViewModel(studentService, groupService, role);
                var teacherViewModel = new TeacherViewModel(teacherService, role);
                var facultyViewModel = new FacultyViewModel(facultyService, role);
                var staffViewModel = new StaffViewModel(staffService, role);
                var scheduleViewModel = new ScheduleViewModel(scheduleService, groupService, teacherService, role);
                var groupViewModel = new GroupViewModel(groupService, facultyService, teacherService, studentService, role);

                var newMainViewModel = new MainViewModel(role, studentViewModel, teacherViewModel, groupViewModel, scheduleViewModel, facultyViewModel, staffViewModel);
                var newMainWindow = new MainWindow(role)
                {
                    DataContext = newMainViewModel
                };

                loginWindow.Close();
                newMainWindow.Show();
            };

            loginWindow.Show();

            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        private async void SetCurrentView(object view, BaseViewModel viewModel)
        {
            if (viewModel is ILoadable loadableViewModel)
            {
                await loadableViewModel.LoadDataAsync();
            }
            CurrentView = view;
        }
    }
}
