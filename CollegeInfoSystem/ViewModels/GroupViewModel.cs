using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public class GroupViewModel : BaseViewModel, ILoadable
{
    private readonly GroupService _groupService;
    private readonly FacultyService _facultyService;
    private readonly TeacherService _teacherService;
    private readonly StudentService _studentService;

    private List<Group> _allGroups;

    public ObservableCollection<Group> Groups { get; set; } = new();
    public ObservableCollection<Student> StudentsInGroup { get; set; } = new();

    private Group _selectedGroup;
    public Group SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            _selectedGroup = value;
            OnPropertyChanged();
            ((RelayCommand)UpdateGroupCommand).NotifyCanExecuteChanged();
            ((RelayCommand)DeleteGroupCommand).NotifyCanExecuteChanged();
            LoadStudentsInGroup();
        }
    }

    private string _groupNameFilter;
    public string GroupNameFilter
    {
        get => _groupNameFilter;
        set
        {
            _groupNameFilter = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    private string _facultyFilter;
    public string FacultyFilter
    {
        get => _facultyFilter;
        set
        {
            _facultyFilter = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    private string _curatorFilter;
    public string CuratorFilter
    {
        get => _curatorFilter;
        set
        {
            _curatorFilter = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    public RelayCommand LoadGroupsCommand { get; }
    public RelayCommand AddGroupCommand { get; }
    public RelayCommand UpdateGroupCommand { get; }
    public RelayCommand DeleteGroupCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public RelayCommand GenerateReportCommand { get; }
    public RelayCommand ImportFromExcelCommand { get; }



    public GroupViewModel(GroupService groupService, FacultyService facultyService, TeacherService teacherService, StudentService studentService)
    {
        _groupService = groupService;
        _facultyService = facultyService;
        _teacherService = teacherService;
        _studentService = studentService;

        LoadGroupsCommand = new RelayCommand(async () => await LoadDataAsync());
        AddGroupCommand = new RelayCommand(AddGroup);
        UpdateGroupCommand = new RelayCommand(UpdateGroup, () => SelectedGroup != null);
        DeleteGroupCommand = new RelayCommand(async () => await DeleteGroupAsync(), () => SelectedGroup != null);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        GenerateReportCommand = new RelayCommand(GenerateReport);
        ImportFromExcelCommand = new RelayCommand(async () => await ImportFromExcel());



        Task.Run(async () => await LoadDataAsync());
    }

    public async Task LoadDataAsync()
    {
        Groups.Clear();
        _allGroups = (await _groupService.GetAllGroupsAsync()).ToList();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allGroups.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(GroupNameFilter))
            filtered = filtered.Where(g => g.GroupName.Contains(GroupNameFilter, System.StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(FacultyFilter))
            filtered = filtered.Where(g => g.Faculty?.FacultyName?.Contains(FacultyFilter, System.StringComparison.OrdinalIgnoreCase) == true);

        if (!string.IsNullOrWhiteSpace(CuratorFilter))
            filtered = filtered.Where(g => g.Curator?.FullName?.Contains(CuratorFilter, System.StringComparison.OrdinalIgnoreCase) == true);

        Groups.Clear();
        foreach (var group in filtered)
            Groups.Add(group);
    }

    private void ClearFilters()
    {
        GroupNameFilter = string.Empty;
        FacultyFilter = string.Empty;
        CuratorFilter = string.Empty;
    }

    private async void LoadStudentsInGroup()
    {
        StudentsInGroup.Clear();
        if (SelectedGroup != null)
        {
            var students = await _studentService.GetStudentsByGroupAsync(SelectedGroup.GroupID);
            foreach (var student in students)
            {
                StudentsInGroup.Add(student);
            }
        }
    }

    private async void AddGroup()
    {
        var newGroup = new Group();
        if (OpenGroupDialog(newGroup))
        {
            await _groupService.AddGroupAsync(newGroup);
            await LoadDataAsync();
        }
    }

    private async void UpdateGroup()
    {
        if (SelectedGroup != null && OpenGroupDialog(SelectedGroup))
        {
            await _groupService.UpdateGroupAsync(SelectedGroup);
            await LoadDataAsync();
        }
    }

    private async Task DeleteGroupAsync()
    {
        if (SelectedGroup != null)
        {
            await _groupService.DeleteGroupAsync(SelectedGroup.GroupID);
            await LoadDataAsync();
        }
    }

    private bool OpenGroupDialog(Group group)
    {
        var viewModel = new GroupDialogViewModel(group, _facultyService, _teacherService);
        var dialog = new GroupDialog { DataContext = viewModel };

        bool isSaved = false;
        viewModel.CloseAction = () =>
        {
            isSaved = viewModel.IsSaved;
            dialog.Close();
        };

        dialog.ShowDialog();
        return isSaved;
    }
    private void GenerateReport()
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Звіт");

        if (SelectedGroup == null)
        {
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Назва групи";
            worksheet.Cell(1, 3).Value = "Факультет";
            worksheet.Cell(1, 4).Value = "Куратор";

            for (int i = 0; i < Groups.Count; i++)
            {
                var group = Groups[i];
                worksheet.Cell(i + 2, 1).Value = group.GroupID;
                worksheet.Cell(i + 2, 2).Value = group.GroupName;
                worksheet.Cell(i + 2, 3).Value = group.Faculty?.FacultyName;
                worksheet.Cell(i + 2, 4).Value = group.Curator?.FullName;
            }
        }
        else
        {
            worksheet.Cell(1, 1).Value = $"Група: {SelectedGroup.GroupName}";
            worksheet.Cell(2, 1).Value = "ID";
            worksheet.Cell(2, 2).Value = "Ім'я";
            worksheet.Cell(2, 3).Value = "Прізвище";

            for (int i = 0; i < StudentsInGroup.Count; i++)
            {
                var student = StudentsInGroup[i];
                worksheet.Cell(i + 3, 1).Value = student.StudentID;
                worksheet.Cell(i + 3, 2).Value = student.FirstName;
                worksheet.Cell(i + 3, 3).Value = student.LastName;
            }
        }

        worksheet.Columns().AdjustToContents();

        var saveDialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = "Групи_Звіт",
            DefaultExt = ".xlsx",
            Filter = "Excel файли (*.xlsx)|*.xlsx"
        };

        if (saveDialog.ShowDialog() == true)
        {
            workbook.SaveAs(saveDialog.FileName);
        }
    }
    private async Task ImportFromExcel()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Excel файли (*.xlsx)|*.xlsx",
            Title = "Виберіть Excel-файл з групами"
        };

        if (dialog.ShowDialog() == true)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook(dialog.FileName);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Пропустити заголовки

            var existingGroups = await _groupService.GetAllGroupsAsync();
            var faculties = await _facultyService.GetAllFacultiesAsync();
            var teachers = await _teacherService.GetAllTeachersAsync();

            int importedCount = 0;
            int duplicateCount = 0;

            foreach (var row in rows)
            {
                var groupName = row.Cell(2).GetString().Trim();
                var facultyName = row.Cell(3).GetString().Trim();
                var curatorFullName = row.Cell(4).GetString().Trim();

                bool exists = existingGroups.Any(g =>
                    g.GroupName.Equals(groupName, System.StringComparison.OrdinalIgnoreCase));

                if (!exists)
                {
                    var faculty = faculties.FirstOrDefault(f =>
                        f.FacultyName.Equals(facultyName, System.StringComparison.OrdinalIgnoreCase));

                    var curator = teachers.FirstOrDefault(t =>
                        t.FullName.Equals(curatorFullName, System.StringComparison.OrdinalIgnoreCase));

                    var newGroup = new Group
                    {
                        GroupName = groupName,
                        FacultyID = faculty?.FacultyID ?? 0,
                        CuratorID = curator?.TeacherID
                    };

                    await _groupService.AddGroupAsync(newGroup);
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
