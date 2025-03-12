using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CollegeInfoSystem.ViewModels;

public class ScheduleViewModel : BaseViewModel
{
    private readonly ScheduleService _scheduleService;
    private Schedule _selectedSchedule;

    public ObservableCollection<Schedule> Schedules { get; set; } = new();

    public Schedule SelectedSchedule
    {
        get => _selectedSchedule;
        set
        {
            _selectedSchedule = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadSchedulesCommand { get; }
    public ICommand AddScheduleCommand { get; }
    public ICommand UpdateScheduleCommand { get; }
    public ICommand DeleteScheduleCommand { get; }

    public ScheduleViewModel(ScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
        LoadSchedulesCommand = new RelayCommand(async () => await LoadSchedulesAsync());
        AddScheduleCommand = new RelayCommand(async () => await AddScheduleAsync());
        UpdateScheduleCommand = new RelayCommand(async () => await UpdateScheduleAsync(), () => SelectedSchedule != null);
        DeleteScheduleCommand = new RelayCommand(async () => await DeleteScheduleAsync(), () => SelectedSchedule != null);
    }

    public async Task LoadSchedulesAsync()
    {
        Schedules.Clear();
        var schedules = await _scheduleService.GetAllSchedulesAsync();
        foreach (var schedule in schedules)
        {
            Schedules.Add(schedule);
        }
    }

    private async Task AddScheduleAsync()
    {
        var newSchedule = new Schedule
        {
            GroupID = 1,
            TeacherID = 1,
            Subject = "Новий предмет",
            DayOfWeek = "Monday",
            StartTime = TimeSpan.Parse("08:00"), 
            EndTime = TimeSpan.Parse("09:30"),   
            Room = "101"
        };
        await _scheduleService.AddScheduleAsync(newSchedule);
        await LoadSchedulesAsync();
    }


    private async Task UpdateScheduleAsync()
    {
        if (SelectedSchedule != null)
        {
            await _scheduleService.UpdateScheduleAsync(SelectedSchedule);
            await LoadSchedulesAsync();
        }
    }

    private async Task DeleteScheduleAsync()
    {
        if (SelectedSchedule != null)
        {
            await _scheduleService.DeleteScheduleAsync(SelectedSchedule.ScheduleID);
            await LoadSchedulesAsync();
        }
    }
}
