using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

public class ScheduleDialogViewModel : BaseViewModel
{
    private Schedule _schedule;
    private readonly GroupService _groupService;
    private readonly TeacherService _teacherService;

    public Action CloseAction { get; set; }
    public bool IsSaved { get; private set; } = false;

    public ObservableCollection<Group> Groups { get; set; } = new();
    public ObservableCollection<Teacher> Teachers { get; set; } = new();

    private Group _selectedGroup;
    public Group SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            _selectedGroup = value;
            _schedule.GroupID = value?.GroupID ?? 0;
            OnPropertyChanged();
        }
    }

    private Teacher _selectedTeacher;
    public Teacher SelectedTeacher
    {
        get => _selectedTeacher;
        set
        {
            _selectedTeacher = value;
            _schedule.TeacherID = value?.TeacherID ?? 0;
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
        var groups = await _groupService.GetAllGroupsAsync();
        var teachers = await _teacherService.GetAllTeachersAsync();

        Application.Current.Dispatcher.Invoke(() =>
        {
            Groups.Clear();
            foreach (var group in groups)
                Groups.Add(group);

            Teachers.Clear();
            foreach (var teacher in teachers)
                Teachers.Add(teacher);

            // Призначення вибраних елементів
            SelectedGroup = Groups.FirstOrDefault(g => g.GroupID == _schedule.GroupID);
            SelectedTeacher = Teachers.FirstOrDefault(t => t.TeacherID == _schedule.TeacherID);
        });

        OnPropertyChanged(nameof(Groups));
        OnPropertyChanged(nameof(Teachers));
    }

    private bool ValidateFields()
    {
        return SelectedGroup != null &&
               SelectedTeacher != null &&
               !string.IsNullOrWhiteSpace(Subject) &&
               !string.IsNullOrWhiteSpace(DayOfWeek) &&
               StartTime != default &&
               EndTime != default &&
               !string.IsNullOrWhiteSpace(Room);
    }

    private void Save()
    {
        if (!ValidateFields())
        {
            MessageBox.Show("Усі поля мають бути заповнені!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsSaved = true;
        CloseAction?.Invoke();
    }

    private void Cancel()
    {
        IsSaved = false;
        CloseAction?.Invoke();
    }
}
