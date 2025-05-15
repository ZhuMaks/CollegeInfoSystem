using ClosedXML.Excel;
using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.ViewModels;
public class TeacherViewModel : BaseViewModel, ILoadable
{
    private readonly TeacherService _teacherService;

    public ObservableCollection<Teacher> Teachers { get; set; } = new();
    private ObservableCollection<Teacher> _allTeachers = new();

    private Teacher _selectedTeacher;
    public Teacher SelectedTeacher
    {
        get => _selectedTeacher;
        set
        {
            _selectedTeacher = value;
            OnPropertyChanged();
            ((RelayCommand)UpdateTeacherCommand).NotifyCanExecuteChanged();
            ((RelayCommand)DeleteTeacherCommand).NotifyCanExecuteChanged();
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

    public RelayCommand LoadTeachersCommand { get; }
    public RelayCommand AddTeacherCommand { get; }
    public RelayCommand UpdateTeacherCommand { get; }
    public RelayCommand DeleteTeacherCommand { get; }
    public RelayCommand ClearFilterCommand { get; }
    public RelayCommand ExportToExcelCommand { get; }
    public RelayCommand ImportFromExcelCommand { get; }

    public TeacherViewModel(TeacherService teacherService)
    {
        _teacherService = teacherService;
        LoadTeachersCommand = new RelayCommand(async () => await LoadDataAsync());
        AddTeacherCommand = new RelayCommand(AddTeacher);
        UpdateTeacherCommand = new RelayCommand(UpdateTeacher, () => SelectedTeacher != null);
        DeleteTeacherCommand = new RelayCommand(async () => await DeleteTeacherAsync(), () => SelectedTeacher != null);
        ClearFilterCommand = new RelayCommand(ClearFilters);
        ExportToExcelCommand = new RelayCommand(ExportToExcel);
        ImportFromExcelCommand = new RelayCommand(async () => await ImportFromExcel());

        Task.Run(async () => await LoadDataAsync());
    }

    public async Task LoadDataAsync()
    {
        _allTeachers.Clear();
        Teachers.Clear();

        var teachers = await _teacherService.GetAllTeachersAsync();
        foreach (var teacher in teachers)
        {
            _allTeachers.Add(teacher);
        }

        ApplyFilters();
    }

    private void ClearFilters()
    {
        LastNameFilter = string.Empty;
        IsCuratorOnly = false;
    }

    private void ApplyFilters()
    {
        var filtered = _allTeachers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(LastNameFilter))
            filtered = filtered.Where(t => t.LastName.Contains(LastNameFilter, System.StringComparison.OrdinalIgnoreCase));

        if (IsCuratorOnly)
            filtered = filtered.Where(t => t.IsCurator);

        Teachers.Clear();
        foreach (var teacher in filtered)
        {
            Teachers.Add(teacher);
        }
    }

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
        if (SelectedTeacher != null)
        {
            await _teacherService.DeleteTeacherAsync(SelectedTeacher.TeacherID);
            await LoadDataAsync();
        }
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

            // Заголовки
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
            Title = "Виберіть Excel-файл з викладачами"
        };

        if (dialog.ShowDialog() == true)
        {
            using var workbook = new XLWorkbook(dialog.FileName);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Пропустити заголовки

            var existingTeachers = await _teacherService.GetAllTeachersAsync();

            int importedCount = 0;
            int duplicateCount = 0;

            foreach (var row in rows)
            {
                var firstName = row.Cell(2).GetString().Trim();
                var lastName = row.Cell(3).GetString().Trim();
                var email = row.Cell(4).GetString().Trim();
                var isCurator = row.Cell(5).GetString().Trim().ToLower() == "так";
                var phone = row.Cell(6).GetString().Trim();

                bool exists = existingTeachers.Any(t =>
                    t.FirstName.Equals(firstName, System.StringComparison.OrdinalIgnoreCase) &&
                    t.LastName.Equals(lastName, System.StringComparison.OrdinalIgnoreCase) &&
                    t.Email.Equals(email, System.StringComparison.OrdinalIgnoreCase)
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
                    importedCount++;
                }
                else
                {
                    duplicateCount++;
                }
            }

            await LoadDataAsync();

            System.Windows.MessageBox.Show(
                $"Імпорт завершено:\nДодано: {importedCount}\nПропущено (дублікати): {duplicateCount}",
                "Результат імпорту",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information
            );
        }
    }
}
