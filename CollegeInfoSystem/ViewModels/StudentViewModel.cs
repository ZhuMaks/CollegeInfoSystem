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

namespace CollegeInfoSystem.ViewModels;
public class StudentViewModel : BaseViewModel, ILoadable
{
    private readonly StudentService _studentService;
    private readonly GroupService _groupService;

    public ObservableCollection<Student> Students { get; set; } = new();
    private List<Student> _allStudents = new();

    public ObservableCollection<Group> Groups { get; set; } = new();

    private Student _selectedStudent;
    public Student SelectedStudent
    {
        get => _selectedStudent;
        set
        {
            _selectedStudent = value;
            OnPropertyChanged();
            ((RelayCommand)UpdateStudentCommand).NotifyCanExecuteChanged();
            ((RelayCommand)DeleteStudentCommand).NotifyCanExecuteChanged();
        }
    }

    private string _searchText;
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            ApplyFilters();
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

    public RelayCommand LoadStudentsCommand { get; }
    public RelayCommand AddStudentCommand { get; }
    public RelayCommand UpdateStudentCommand { get; }
    public RelayCommand DeleteStudentCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public RelayCommand ExportToExcelCommand { get; }
    public RelayCommand ImportFromExcelCommand { get; }


    public StudentViewModel(StudentService studentService, GroupService groupService)
    {
        _studentService = studentService;
        _groupService = groupService;

        LoadStudentsCommand = new RelayCommand(async () => await LoadDataAsync());
        AddStudentCommand = new RelayCommand(AddStudent);
        UpdateStudentCommand = new RelayCommand(UpdateStudent, () => SelectedStudent != null);
        DeleteStudentCommand = new RelayCommand(async () => await DeleteStudentAsync(), () => SelectedStudent != null);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        ExportToExcelCommand = new RelayCommand(ExportToExcel);
        ImportFromExcelCommand = new RelayCommand(ImportFromExcel);


        Task.Run(async () => await LoadDataAsync());
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedGroup = null;
        ApplyFilters();
    }

    public async Task LoadDataAsync()
    {
        Students.Clear();
        Groups.Clear();
        _allStudents = (await _studentService.GetAllStudentsAsync()).ToList();
        var allGroups = await _groupService.GetAllGroupsAsync();

        foreach (var g in allGroups)
            Groups.Add(g);

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allStudents.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(s =>
                s.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                s.StudentID.ToString().Contains(SearchText));
        }

        if (SelectedGroup != null)
            filtered = filtered.Where(s => s.Group?.GroupID == SelectedGroup.GroupID);

        Students.Clear();
        foreach (var s in filtered)
            Students.Add(s);
    }

    private async void AddStudent()
    {
        var newStudent = new Student();
        if (OpenStudentDialog(newStudent))
        {
            await _studentService.AddStudentAsync(newStudent);
            await LoadDataAsync();
        }
    }

    private async void UpdateStudent()
    {
        if (SelectedStudent != null && OpenStudentDialog(SelectedStudent))
        {
            await _studentService.UpdateStudentAsync(SelectedStudent);
            await LoadDataAsync();
        }
    }

    private async Task DeleteStudentAsync()
    {
        if (SelectedStudent != null)
        {
            await _studentService.DeleteStudentAsync(SelectedStudent.StudentID);
            await LoadDataAsync();
        }
    }

    private bool OpenStudentDialog(Student student)
    {
        var viewModel = new StudentDialogViewModel(student, _groupService);
        var dialog = new StudentDialog { DataContext = viewModel };

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
            FileName = "Students_Report.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Студенти");

            // Заголовки
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Ім'я";
            worksheet.Cell(1, 3).Value = "Прізвище";
            worksheet.Cell(1, 4).Value = "Email";
            worksheet.Cell(1, 5).Value = "Група";
            worksheet.Cell(1, 6).Value = "Телефон";
            worksheet.Cell(1, 7).Value = "Дата народження";
            worksheet.Cell(1, 8).Value = "Адреса";

            // Дані
            for (int i = 0; i < Students.Count; i++)
            {
                var s = Students[i];
                worksheet.Cell(i + 2, 1).Value = s.StudentID;
                worksheet.Cell(i + 2, 2).Value = s.FirstName;
                worksheet.Cell(i + 2, 3).Value = s.LastName;
                worksheet.Cell(i + 2, 4).Value = s.Email;
                worksheet.Cell(i + 2, 5).Value = s.Group?.GroupName ?? "";
                worksheet.Cell(i + 2, 6).Value = s.Phone;
                worksheet.Cell(i + 2, 7).Value = s.DateOfBirth.ToShortDateString();
                worksheet.Cell(i + 2, 8).Value = s.Address;
            }

            // Автоширина колонок
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(dialog.FileName);
        }
    }
    private async void ImportFromExcel()
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
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Пропускаємо заголовок

                var existingStudents = await _studentService.GetAllStudentsAsync();

                int importedCount = 0;
                int duplicateCount = 0;

                foreach (var row in rows)
                {
                    var firstName = row.Cell(1).GetString().Trim();
                    var lastName = row.Cell(2).GetString().Trim();
                    var email = row.Cell(3).GetString().Trim();
                    var phone = row.Cell(4).GetString().Trim();
                    var dateOfBirth = row.Cell(5).GetDateTime();
                    var address = row.Cell(6).GetString().Trim();
                    var groupName = row.Cell(7).GetString().Trim();

                    // Перевірка на дублікати
                    bool exists = existingStudents.Any(s =>
                        s.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                        s.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase) &&
                        s.Email.Equals(email, StringComparison.OrdinalIgnoreCase)
                    );

                    if (!exists)
                    {
                        var group = Groups.FirstOrDefault(g =>
                            g.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));

                        var student = new Student
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            Email = email,
                            Phone = phone,
                            DateOfBirth = dateOfBirth,
                            Address = address,
                            GroupID = group?.GroupID ?? 0
                        };

                        await _studentService.AddStudentAsync(student);
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
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Помилка при імпорті: " + ex.Message);
            }
        }
    }


}