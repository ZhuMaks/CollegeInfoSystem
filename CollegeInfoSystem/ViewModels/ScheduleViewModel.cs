using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Input;
using ClosedXML.Excel;
using Microsoft.Win32;
using System;

namespace CollegeInfoSystem.ViewModels;

public class ScheduleViewModel : BaseViewModel, ILoadable
{
    private readonly ScheduleService _scheduleService;
    private readonly GroupService _groupService;
    private readonly TeacherService _teacherService;
    private DispatcherTimer _refreshTimer;

    public ObservableCollection<Schedule> Schedules { get; set; } = new();
    public ObservableCollection<Schedule> SelectedSchedules { get; set; } = new();
    private List<Schedule> _allSchedules = new();

    public ObservableCollection<string> DaysOfWeek { get; } = new(new[] { "", "Понеділок", "Вівторок", "Середа", "Четвер", "П'ятниця", "Субота", "Неділя" });
    public ObservableCollection<Group> Groups { get; } = new();
    public ObservableCollection<Teacher> Teachers { get; } = new();

    private string _currentUserRole;
    private bool _isEditing = false;

    public string CurrentUserRole
    {
        get => _currentUserRole;
        set
        {
            _currentUserRole = value;
            OnPropertyChanged();
            UpdateCommandsCanExecute();
        }
    }

    public RelayCommand LoadSchedulesCommand { get; }
    public RelayCommand AddScheduleCommand { get; }
    public RelayCommand UpdateScheduleCommand { get; }
    public RelayCommand DeleteScheduleCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public RelayCommand ExportToExcelCommand { get; }
    public RelayCommand ImportFromExcelCommand { get; }

    public ScheduleViewModel(ScheduleService scheduleService, GroupService groupService, TeacherService teacherService, string currentUserRole)
    {
        _scheduleService = scheduleService;
        _groupService = groupService;
        _teacherService = teacherService;
        _currentUserRole = currentUserRole;

        LoadSchedulesCommand = new RelayCommand(async () => await LoadDataAsync());
        AddScheduleCommand = new RelayCommand(AddSchedule, CanExecuteAdd);
        UpdateScheduleCommand = new RelayCommand(UpdateSchedule, CanExecuteUpdate);
        DeleteScheduleCommand = new RelayCommand(async () => await DeleteScheduleAsync(), CanExecuteDelete);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        ExportToExcelCommand = new RelayCommand(ExportToExcel, () => true);
        ImportFromExcelCommand = new RelayCommand(ImportFromExcel, CanExecuteImport);

        Task.Run(async () => await LoadDataAsync());

        _refreshTimer = new DispatcherTimer();
        _refreshTimer.Interval = TimeSpan.FromSeconds(15);
        _refreshTimer.Tick += RefreshTimer_Tick;
        _refreshTimer.Start();
    }
    private async void RefreshTimer_Tick(object? sender, EventArgs e)
    {
        if (_isEditing) return;

        _refreshTimer.Stop();
        try
        {
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Помилка при оновленні: " + ex.Message);
        }
        finally
        {
            _refreshTimer.Start();
        }
    }


    private void UpdateCommandsCanExecute()
    {
        AddScheduleCommand.NotifyCanExecuteChanged();
        UpdateScheduleCommand.NotifyCanExecuteChanged();
        DeleteScheduleCommand.NotifyCanExecuteChanged();
        ImportFromExcelCommand.NotifyCanExecuteChanged();
    }

    private Schedule _selectedSchedule;
    public Schedule SelectedSchedule
    {
        get => _selectedSchedule;
        set
        {
            _selectedSchedule = value;
            OnPropertyChanged();
            UpdateCommandsCanExecute();
        }
    }

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

    public async Task LoadDataAsync()
    {
        var schedules = await _scheduleService.GetAllSchedulesAsync();
        var groups = await _groupService.GetAllGroupsAsync();
        var teachers = await _teacherService.GetAllTeachersAsync();

        _allSchedules = schedules.ToList();

        var selectedGroupId = SelectedGroup?.GroupID;
        var selectedTeacherId = SelectedTeacher?.TeacherID;
        var selectedDay = SelectedDay;

        Groups.Clear();
        foreach (var group in groups)
            Groups.Add(group);

        Teachers.Clear();
        foreach (var teacher in teachers)
            Teachers.Add(teacher);

        SelectedGroup = Groups.FirstOrDefault(g => g.GroupID == selectedGroupId);
        SelectedTeacher = Teachers.FirstOrDefault(t => t.TeacherID == selectedTeacherId);
        SelectedDay = DaysOfWeek.Contains(selectedDay) ? selectedDay : "";

        ApplyFilters();
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

    private void ClearFilters()
    {
        SelectedGroup = null;
        SelectedTeacher = null;
        SelectedDay = null;
        ApplyFilters();
    }

    private bool CanExecuteAdd() => CurrentUserRole is "admin";
    private bool CanExecuteUpdate() => SelectedSchedule != null && CurrentUserRole is "admin";
    private bool CanExecuteDelete() =>
        (SelectedSchedule != null || SelectedSchedules.Any()) && CurrentUserRole is "admin";
    private bool CanExecuteImport() => CurrentUserRole == "admin";

    private async void AddSchedule()
    {
        _isEditing = true;

        try
        {
            var newSchedule = new Schedule();
            if (OpenScheduleDialog(newSchedule))
            {
                await _scheduleService.AddScheduleAsync(newSchedule);
                await LoadDataAsync();
            }
        }
        finally
        {
            _isEditing = false;
        }
    }


    private async void UpdateSchedule()
    {
        if (SelectedSchedule == null)
            return;

        _isEditing = true;

        try
        {
            if (OpenScheduleDialog(SelectedSchedule))
            {
                await _scheduleService.UpdateScheduleAsync(SelectedSchedule);
                await LoadDataAsync();
            }
        }
        finally
        {
            _isEditing = false;
        }
    }

    private async Task DeleteScheduleAsync()
    {
        _isEditing = true;

        try
        {
            if (SelectedSchedules.Any())
            {
                foreach (var schedule in SelectedSchedules.ToList())
                {
                    await _scheduleService.DeleteScheduleAsync(schedule.ScheduleID);
                }
                SelectedSchedules.Clear();
            }
            else if (SelectedSchedule != null)
            {
                await _scheduleService.DeleteScheduleAsync(SelectedSchedule.ScheduleID);
            }

            await LoadDataAsync();
        }
        finally
        {
            _isEditing = false;
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
            FileName = "Schedules_Report.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Розклад");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Група";
            worksheet.Cell(1, 3).Value = "Викладач";
            worksheet.Cell(1, 4).Value = "Предмет";
            worksheet.Cell(1, 5).Value = "День тижня";
            worksheet.Cell(1, 6).Value = "Час початку";
            worksheet.Cell(1, 7).Value = "Час завершення";
            worksheet.Cell(1, 8).Value = "Аудиторія";

            for (int i = 0; i < Schedules.Count; i++)
            {
                var s = Schedules[i];
                worksheet.Cell(i + 2, 1).Value = s.ScheduleID;
                worksheet.Cell(i + 2, 2).Value = s.Group?.GroupName ?? "";
                worksheet.Cell(i + 2, 3).Value = $"{s.Teacher?.LastName} {s.Teacher?.FirstName}";
                worksheet.Cell(i + 2, 4).Value = s.Subject;
                worksheet.Cell(i + 2, 5).Value = s.DayOfWeek;
                worksheet.Cell(i + 2, 6).Value = s.StartTime.ToString(@"hh\:mm");
                worksheet.Cell(i + 2, 7).Value = s.EndTime.ToString(@"hh\:mm");
                worksheet.Cell(i + 2, 8).Value = s.Room;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);
        }
    }

    private async void ImportFromExcel()
    {
        _isEditing = true;

        try
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Виберіть файл Excel"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using var workbook = new XLWorkbook(dialog.FileName);
                    var worksheet = workbook.Worksheets.First();
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                    var allGroups = Groups.ToList();
                    var allTeachers = Teachers.ToList();

                    int imported = 0;
                    int skipped = 0;

                    foreach (var row in rows)
                    {
                        var groupName = row.Cell(1).GetString().Trim();
                        var teacherName = row.Cell(2).GetString().Trim();
                        var subject = row.Cell(3).GetString().Trim();
                        var day = row.Cell(4).GetString().Trim();

                        if (!row.Cell(5).TryGetValue(out DateTime startDateTime))
                        {
                            System.Windows.MessageBox.Show($"Невірний формат часу початку у рядку {row.RowNumber()}");
                            skipped++;
                            continue;
                        }
                        var startTime = startDateTime.TimeOfDay;

                        if (!row.Cell(6).TryGetValue(out DateTime endDateTime))
                        {
                            System.Windows.MessageBox.Show($"Невірний формат часу завершення у рядку {row.RowNumber()}");
                            skipped++;
                            continue;
                        }
                        var endTime = endDateTime.TimeOfDay;

                        var room = row.Cell(7).GetString().Trim();

                        var group = allGroups.FirstOrDefault(g => g.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));
                        if (group == null)
                        {
                            System.Windows.MessageBox.Show($"Групу '{groupName}' не знайдено у базі. Рядок {row.RowNumber()} пропущено.");
                            skipped++;
                            continue;
                        }

                        var teacher = allTeachers.FirstOrDefault(t =>
                            $"{t.LastName} {t.FirstName}".Equals(teacherName, StringComparison.OrdinalIgnoreCase) ||
                            $"{t.FirstName} {t.LastName}".Equals(teacherName, StringComparison.OrdinalIgnoreCase));

                        if (teacher == null)
                        {
                            System.Windows.MessageBox.Show($"Викладача '{teacherName}' не знайдено у базі. Рядок {row.RowNumber()} пропущено.");
                            skipped++;
                            continue;
                        }

                        var schedule = new Schedule
                        {
                            GroupID = group.GroupID,
                            TeacherID = teacher.TeacherID,
                            Subject = subject,
                            DayOfWeek = day,
                            StartTime = startTime,
                            EndTime = endTime,
                            Room = room
                        };

                        var duplicateExists = _allSchedules.Any(s =>
                            s.GroupID == schedule.GroupID &&
                            s.TeacherID == schedule.TeacherID &&
                            s.Subject.Equals(schedule.Subject, StringComparison.OrdinalIgnoreCase) &&
                            s.DayOfWeek.Equals(schedule.DayOfWeek, StringComparison.OrdinalIgnoreCase) &&
                            s.StartTime == schedule.StartTime &&
                            s.EndTime == schedule.EndTime &&
                            s.Room.Equals(schedule.Room, StringComparison.OrdinalIgnoreCase));

                        if (duplicateExists)
                        {
                            skipped++;
                            continue;
                        }

                        try
                        {
                            await _scheduleService.AddScheduleAsync(schedule);
                            _allSchedules.Add(schedule); // Додай вручну в локальний кеш
                            imported++;
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show($"Помилка додавання розкладу у рядку {row.RowNumber()}: {ex.Message}");
                            skipped++;
                        }
                    }

                    await LoadDataAsync();

                    System.Windows.MessageBox.Show($"Імпорт завершено. Додано записів: {imported}, пропущено: {skipped}", "Успішно", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Помилка при імпорті: " + ex.Message);
                }
            }
        }
        finally
        {
            _isEditing = false;
        }
    }
}
