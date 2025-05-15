using System.Windows.Input;
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

        private readonly StudentViewModel _studentViewModel;
        private readonly TeacherViewModel _teacherViewModel;
        private readonly GroupViewModel _groupViewModel;
        private readonly ScheduleViewModel _scheduleViewModel;
        private readonly FacultyViewModel _facultyViewModel;
        private readonly StaffViewModel _staffViewModel;

        public ICommand OpenStudentsViewCommand { get; }
        public ICommand OpenTeachersViewCommand { get; }
        public ICommand OpenGroupsViewCommand { get; }
        public ICommand OpenScheduleViewCommand { get; }
        public ICommand OpenFacultyViewCommand { get; }
        public ICommand OpenStaffViewCommand { get; }

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
            OpenTeachersViewCommand = new RelayCommand(() => SetCurrentView(_teachersView, _teacherViewModel), () => _userRole != "guest");
            OpenGroupsViewCommand = new RelayCommand(() => SetCurrentView(_groupsView, _groupViewModel), () => _userRole != "guest");
            OpenScheduleViewCommand = new RelayCommand(() => SetCurrentView(_scheduleView, _scheduleViewModel));
            OpenFacultyViewCommand = new RelayCommand(() => SetCurrentView(_facultyView, _facultyViewModel), () => _userRole != "guest");
            OpenStaffViewCommand = new RelayCommand(() => SetCurrentView(_staffView, _staffViewModel), () => _userRole != "guest");

            CurrentView = _studentsView;
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
