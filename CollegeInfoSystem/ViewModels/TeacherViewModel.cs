using ClosedXML.Excel;
using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace CollegeInfoSystem.ViewModels;

public class TeacherViewModel : BaseViewModel, ILoadable
{
    private readonly TeacherService _teacherService;

    public ObservableCollection<Teacher> Teachers { get; set; } = new();
    public ObservableCollection<Teacher> SelectedTeachers { get; set; } = new();
    private ObservableCollection<Teacher> _allTeachers = new();

    private Teacher _selectedTeacher;
    public Teacher SelectedTeacher
    {
        get => _selectedTeacher;
        set
        {
            _selectedTeacher = value;
            OnPropertyChanged();
            UpdateTeacherCommand.NotifyCanExecuteChanged();
            DeleteTeacherCommand.NotifyCanExecuteChanged();
        }
    }

    private string _lastNameFilter;
    public string LastNameFilter
    {
        get => _lastNameFilter;
        set
        {
            _lastNameFilter = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    private bool _isCuratorOnly;
    public bool IsCuratorOnly
    {
        get => _isCuratorOnly;
        set
        {
            _isCuratorOnly = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    private string _currentUserRole;
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

    public RelayCommand LoadTeachersCommand { get; }
    public RelayCommand AddTeacherCommand { get; }
    public RelayCommand UpdateTeacherCommand { get; }
    public RelayCommand DeleteTeacherCommand { get; }
    public RelayCommand ClearFilterCommand { get; }
    public RelayCommand ExportToExcelCommand { get; }
    public RelayCommand ImportFromExcelCommand { get; }

    public TeacherViewModel(TeacherService teacherService, string currentUserRole)
    {
        _teacherService = teacherService;
        _currentUserRole = currentUserRole;

        LoadTeachersCommand = new RelayCommand(async () => await LoadDataAsync());
        AddTeacherCommand = new RelayCommand(AddTeacher, CanExecuteAddTeacher);
        UpdateTeacherCommand = new RelayCommand(UpdateTeacher, CanExecuteUpdateTeacher);
        DeleteTeacherCommand = new RelayCommand(async () => await DeleteTeacherAsync(), CanExecuteDeleteTeacher);
        ClearFilterCommand = new RelayCommand(ClearFilters);
        ExportToExcelCommand = new RelayCommand(ExportToExcel, CanExecuteExport);
        ImportFromExcelCommand = new RelayCommand(async () => await ImportFromExcel(), CanExecuteImport);

        Task.Run(async () => await LoadDataAsync());
    }

    private void UpdateCommandsCanExecute()
    {
        AddTeacherCommand.NotifyCanExecuteChanged();
        UpdateTeacherCommand.NotifyCanExecuteChanged();
        DeleteTeacherCommand.NotifyCanExecuteChanged();
        ExportToExcelCommand.NotifyCanExecuteChanged();
        ImportFromExcelCommand.NotifyCanExecuteChanged();
    }

    public async Task LoadDataAsync()
    {
        _allTeachers.Clear();
        Teachers.Clear();

        var teachers = await _teacherService.GetAllTeachersAsync();
        foreach (var teacher in teachers)
            _allTeachers.Add(teacher);

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allTeachers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(LastNameFilter))
            filtered = filtered.Where(t => t.LastName.Contains(LastNameFilter, StringComparison.OrdinalIgnoreCase));

        if (IsCuratorOnly)
            filtered = filtered.Where(t => t.IsCurator);

        Teachers.Clear();
        foreach (var teacher in filtered)
            Teachers.Add(teacher);
    }

    private void ClearFilters()
    {
        LastNameFilter = string.Empty;
        IsCuratorOnly = false;
    }

    private bool CanExecuteAddTeacher() => CurrentUserRole == "admin";
    private bool CanExecuteUpdateTeacher() => SelectedTeacher != null && CurrentUserRole == "admin";
    private bool CanExecuteDeleteTeacher() =>
        (SelectedTeacher != null || SelectedTeachers.Count > 1) &&
        CurrentUserRole is "admin" or "teacher";
    private bool CanExecuteExport() => CurrentUserRole is "admin" or "teacher" or "guest";
    private bool CanExecuteImport() => CurrentUserRole == "admin";

    private async void AddTeacher()
    {
        var newTeacher = new Teacher();
        if (OpenTeacherDialog(newTeacher))
        {
            await _teacherService.AddTeacherAsync(newTeacher);
            await LoadDataAsync();
        }
    }

    private async void UpdateTeacher()
    {
        if (SelectedTeacher != null && OpenTeacherDialog(SelectedTeacher))
        {
            await _teacherService.UpdateTeacherAsync(SelectedTeacher);
            await LoadDataAsync();
        }
    }

    private async Task DeleteTeacherAsync()
    {
        if (SelectedTeachers?.Count > 1)
        {
            var idsToDelete = SelectedTeachers.Select(t => t.TeacherID).ToList();
            foreach (var id in idsToDelete)
                await _teacherService.DeleteTeacherAsync(id);
        }
        else if (SelectedTeacher != null)
        {
            await _teacherService.DeleteTeacherAsync(SelectedTeacher.TeacherID);
        }

        await LoadDataAsync();
    }


    private bool OpenTeacherDialog(Teacher teacher)
    {
        var viewModel = new TeacherDialogViewModel(teacher);
        var dialog = new TeacherDialog { DataContext = viewModel };

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
            FileName = "Teachers_Report.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Викладачі");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Ім'я";
            worksheet.Cell(1, 3).Value = "Прізвище";
            worksheet.Cell(1, 4).Value = "Email";
            worksheet.Cell(1, 5).Value = "Куратор";
            worksheet.Cell(1, 6).Value = "Телефон";

            for (int i = 0; i < Teachers.Count; i++)
            {
                var t = Teachers[i];
                worksheet.Cell(i + 2, 1).Value = t.TeacherID;
                worksheet.Cell(i + 2, 2).Value = t.FirstName;
                worksheet.Cell(i + 2, 3).Value = t.LastName;
                worksheet.Cell(i + 2, 4).Value = t.Email;
                worksheet.Cell(i + 2, 5).Value = t.IsCurator ? "Так" : "Ні";
                worksheet.Cell(i + 2, 6).Value = t.Phone;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);
        }
    }

    private async Task ImportFromExcel()
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

                var existingTeachers = await _teacherService.GetAllTeachersAsync();

                int imported = 0;
                int duplicates = 0;

                foreach (var row in rows)
                {
                    var firstName = row.Cell(1).GetString().Trim();
                    var lastName = row.Cell(2).GetString().Trim();
                    var email = row.Cell(3).GetString().Trim();
                    var isCurator = row.Cell(4).GetString().Trim().Equals("Так", StringComparison.OrdinalIgnoreCase);
                    var phone = row.Cell(5).GetString().Trim();

                    bool exists = existingTeachers.Any(t =>
                        t.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                        t.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase) &&
                        t.Email.Equals(email, StringComparison.OrdinalIgnoreCase)
                    );

                    if (!exists)
                    {
                        var teacher = new Teacher
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            Email = email,
                            IsCurator = isCurator,
                            Phone = phone
                        };

                        await _teacherService.AddTeacherAsync(teacher);
                        imported++;
                    }
                    else
                    {
                        duplicates++;
                    }
                }

                await LoadDataAsync();

                System.Windows.MessageBox.Show(
                    $"Імпорт завершено:\nДодано: {imported}\nПропущено (дублікати): {duplicates}",
                    "Результат імпорту",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Помилка при імпорті: " + ex.Message);
            }
        }
    }
}
