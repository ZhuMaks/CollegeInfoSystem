using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using ClosedXML.Excel;
using Microsoft.Win32;

public class ScheduleViewModel : BaseViewModel, ILoadable
{
    private readonly ScheduleService _scheduleService;
    private readonly GroupService _groupService;
    private readonly TeacherService _teacherService;

    public ObservableCollection<Schedule> Schedules { get; set; } = new();
    private List<Schedule> _allSchedules = new();

    public ObservableCollection<string> DaysOfWeek { get; } = new(new[] { "", "Понеділок", "Вівторок", "Середа", "Четвер", "П’ятниця", "Субота", "Неділя" });
    public ObservableCollection<Group> Groups { get; } = new();
    public ObservableCollection<Teacher> Teachers { get; } = new();

    private Group _selectedGroup;
    public Group SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            _selectedGroup = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    private Teacher _selectedTeacher;
    public Teacher SelectedTeacher
    {
        get => _selectedTeacher;
        set
        {
            _selectedTeacher = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    private string _selectedDay;
    public string SelectedDay
    {
        get => _selectedDay;
        set
        {
            _selectedDay = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    public ScheduleViewModel(ScheduleService scheduleService, GroupService groupService, TeacherService teacherService)
    {
        _scheduleService = scheduleService;
        _groupService = groupService;
        _teacherService = teacherService;

        LoadSchedulesCommand = new RelayCommand(async () => await LoadDataAsync());
        AddScheduleCommand = new RelayCommand(AddSchedule);
        UpdateScheduleCommand = new RelayCommand(UpdateSchedule, () => SelectedSchedule != null);
        DeleteScheduleCommand = new RelayCommand(async () => await DeleteScheduleAsync(), () => SelectedSchedule != null);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        ExportToExcelCommand = new RelayCommand(ExportToExcel);

        Task.Run(async () => await LoadDataAsync());
    }

    public RelayCommand LoadSchedulesCommand { get; }
    public RelayCommand AddScheduleCommand { get; }
    public RelayCommand UpdateScheduleCommand { get; }
    public RelayCommand DeleteScheduleCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public RelayCommand ExportToExcelCommand { get; }

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

    public async Task LoadDataAsync()
    {
        var schedules = await _scheduleService.GetAllSchedulesAsync();
        var groups = await _groupService.GetAllGroupsAsync();
        var teachers = await _teacherService.GetAllTeachersAsync();

        _allSchedules = schedules.ToList();

        Groups.Clear();
        foreach (var group in groups)
            Groups.Add(group);

        Teachers.Clear();
        foreach (var teacher in teachers)
            Teachers.Add(teacher);

        ApplyFilters();
    }

    private void ClearFilters()
    {
        SelectedGroup = null;
        SelectedTeacher = null;
        SelectedDay = null;
    }

    private void ApplyFilters()
    {
        var filtered = _allSchedules.AsEnumerable();

        if (SelectedGroup != null)
            filtered = filtered.Where(s => s.Group?.GroupID == SelectedGroup.GroupID);

        if (SelectedTeacher != null)
            filtered = filtered.Where(s => s.Teacher?.TeacherID == SelectedTeacher.TeacherID);

        if (!string.IsNullOrEmpty(SelectedDay))
            filtered = filtered.Where(s => s.DayOfWeek == SelectedDay);

        Schedules.Clear();
        foreach (var s in filtered)
            Schedules.Add(s);
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
    private void ExportToExcel()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = "Schedule_Report.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Розклад");

            // Заголовки
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Група";
            worksheet.Cell(1, 3).Value = "Викладач";
            worksheet.Cell(1, 4).Value = "Предмет";
            worksheet.Cell(1, 5).Value = "День";
            worksheet.Cell(1, 6).Value = "Час";
            worksheet.Cell(1, 7).Value = "Аудиторія";

            // Дані
            for (int i = 0; i < Schedules.Count; i++)
            {
                var s = Schedules[i];
                worksheet.Cell(i + 2, 1).Value = s.ScheduleID;
                worksheet.Cell(i + 2, 2).Value = s.Group?.GroupName ?? "";
                worksheet.Cell(i + 2, 3).Value = s.Teacher?.FullName ?? "";
                worksheet.Cell(i + 2, 4).Value = s.Subject;
                worksheet.Cell(i + 2, 5).Value = s.DayOfWeek;
                worksheet.Cell(i + 2, 6).Value = $"{s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}";
                worksheet.Cell(i + 2, 7).Value = s.Room;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);
        }
    }
}
