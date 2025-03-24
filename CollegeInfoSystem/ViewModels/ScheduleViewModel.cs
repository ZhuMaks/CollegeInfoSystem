using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public class ScheduleViewModel : BaseViewModel, ILoadable
{
    private readonly ScheduleService _scheduleService;
    private readonly GroupService _groupService;
    private readonly TeacherService _teacherService;

    public ObservableCollection<Schedule> Schedules { get; set; } = new();
    private Schedule _selectedSchedule;
    public Schedule SelectedSchedule
    {
        get => _selectedSchedule;
        set
        {
            _selectedSchedule = value;
            OnPropertyChanged();
            ((RelayCommand)UpdateScheduleCommand).NotifyCanExecuteChanged();
            ((RelayCommand)DeleteScheduleCommand).NotifyCanExecuteChanged();
        }
    }

    public RelayCommand LoadSchedulesCommand { get; }
    public RelayCommand AddScheduleCommand { get; }
    public RelayCommand UpdateScheduleCommand { get; }
    public RelayCommand DeleteScheduleCommand { get; }

    public ScheduleViewModel(ScheduleService scheduleService, GroupService groupService, TeacherService teacherService)
    {
        _scheduleService = scheduleService;
        _groupService = groupService;
        _teacherService = teacherService;

        LoadSchedulesCommand = new RelayCommand(async () => await LoadDataAsync());
        AddScheduleCommand = new RelayCommand(AddSchedule);
        UpdateScheduleCommand = new RelayCommand(UpdateSchedule, () => SelectedSchedule != null);
        DeleteScheduleCommand = new RelayCommand(async () => await DeleteScheduleAsync(), () => SelectedSchedule != null);

        Task.Run(async () => await LoadDataAsync());
    }

    public async Task LoadDataAsync()
    {
        Schedules.Clear();
        var schedules = await _scheduleService.GetAllSchedulesAsync();
        foreach (var schedule in schedules)
        {
            Schedules.Add(schedule);
        }
    }

    private async void AddSchedule()
    {
        var newSchedule = new Schedule();
        if (OpenScheduleDialog(newSchedule))
        {
            await _scheduleService.AddScheduleAsync(newSchedule);
            await LoadDataAsync();
        }
    }

    private async void UpdateSchedule()
    {
        if (SelectedSchedule != null && OpenScheduleDialog(SelectedSchedule))
        {
            await _scheduleService.UpdateScheduleAsync(SelectedSchedule);
            await LoadDataAsync();
        }
    }

    private async Task DeleteScheduleAsync()
    {
        if (SelectedSchedule != null)
        {
            await _scheduleService.DeleteScheduleAsync(SelectedSchedule.ScheduleID);
            await LoadDataAsync();
        }
    }

    private bool OpenScheduleDialog(Schedule schedule)
    {
        var viewModel = new ScheduleDialogViewModel(schedule, _groupService, _teacherService);
        var dialog = new ScheduleDialog { DataContext = viewModel };

        bool isSaved = false;
        viewModel.CloseAction = () =>
        {
            isSaved = viewModel.IsSaved;
            dialog.Close();
        };

        dialog.ShowDialog();
        return isSaved;
    }
}
