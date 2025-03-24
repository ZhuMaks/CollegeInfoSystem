using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public class ScheduleDialogViewModel : BaseViewModel
{
    private Schedule _schedule;
    private readonly GroupService _groupService;
    private readonly TeacherService _teacherService;

    public Action CloseAction { get; set; }
    public bool IsSaved { get; private set; } = false;

    public ObservableCollection<Group> Groups { get; set; } = new();
    public ObservableCollection<Teacher> Teachers { get; set; } = new();

    public Group SelectedGroup
    {
        get => _schedule.Group;
        set
        {
            _schedule.Group = value;
            OnPropertyChanged();
        }
    }

    public Teacher SelectedTeacher
    {
        get => _schedule.Teacher;
        set
        {
            _schedule.Teacher = value;
            OnPropertyChanged();
        }
    }

    public string Subject
    {
        get => _schedule.Subject;
        set
        {
            _schedule.Subject = value;
            OnPropertyChanged();
        }
    }

    public string DayOfWeek
    {
        get => _schedule.DayOfWeek;
        set
        {
            _schedule.DayOfWeek = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan StartTime
    {
        get => _schedule.StartTime;
        set
        {
            _schedule.StartTime = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan EndTime
    {
        get => _schedule.EndTime;
        set
        {
            _schedule.EndTime = value;
            OnPropertyChanged();
        }
    }

    public string Room
    {
        get => _schedule.Room;
        set
        {
            _schedule.Room = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public ScheduleDialogViewModel(Schedule schedule, GroupService groupService, TeacherService teacherService)
    {
        _schedule = schedule;
        _groupService = groupService;
        _teacherService = teacherService;

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);

        Task.Run(async () => await LoadDataAsync());
    }

    private async Task LoadDataAsync()
    {
        Groups = new ObservableCollection<Group>(await _groupService.GetAllGroupsAsync());
        Teachers = new ObservableCollection<Teacher>(await _teacherService.GetAllTeachersAsync());
    }

    private void Save()
    {
        IsSaved = true;
        CloseAction?.Invoke();
    }

    private void Cancel()
    {
        IsSaved = false;
        CloseAction?.Invoke();
    }
}
