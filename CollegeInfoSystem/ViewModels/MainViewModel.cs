using CollegeInfoSystem.Views;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeInfoSystem.ViewModels;

namespace CollegeInfoSystem.ViewModels;

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

    public MainViewModel(StudentViewModel studentViewModel, TeacherViewModel teacherViewModel, GroupViewModel groupViewModel, ScheduleViewModel scheduleViewModel, FacultyViewModel facultyViewModel, StaffViewModel staffViewModel)
    {
        _studentViewModel = studentViewModel;
        _teacherViewModel = teacherViewModel;
        _groupViewModel = groupViewModel;
        _scheduleViewModel = scheduleViewModel;
        _facultyViewModel = facultyViewModel;
        _staffViewModel = staffViewModel;

        OpenStudentsViewCommand = new RelayCommand(() => CurrentView = new StudentsView { DataContext = _studentViewModel });
        OpenTeachersViewCommand = new RelayCommand(() => CurrentView = new TeachersView { DataContext = _teacherViewModel });
        OpenGroupsViewCommand = new RelayCommand(() => CurrentView = new GroupsView { DataContext = _groupViewModel });
        OpenScheduleViewCommand = new RelayCommand(() => CurrentView = new ScheduleView { DataContext = _scheduleViewModel });
        OpenFacultyViewCommand = new RelayCommand(() => CurrentView = new FacultyView { DataContext = _facultyViewModel });
        OpenStaffViewCommand = new RelayCommand(() => CurrentView = new StaffView { DataContext = _staffViewModel });
        _staffViewModel = staffViewModel;
    }
}
