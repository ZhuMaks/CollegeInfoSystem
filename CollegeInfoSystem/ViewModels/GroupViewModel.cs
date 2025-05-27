using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.ViewModels;

public class GroupViewModel : BaseViewModel, ILoadable
{
    private readonly GroupService _groupService;
    private readonly FacultyService _facultyService;
    private readonly TeacherService _teacherService;
    private readonly StudentService _studentService;
    private DispatcherTimer _refreshTimer;

    private List<Group> _allGroups = new();

    public ObservableCollection<Group> Groups { get; set; } = new();
    public ObservableCollection<Student> StudentsInGroup { get; set; } = new();
    public ObservableCollection<Group> SelectedGroups { get; set; } = new();
    public ObservableCollection<Faculty> Faculties { get; set; } = new();
    private Faculty _selectedFaculty;
    private bool _isEditing = false;

    public Faculty SelectedFaculty
    {
        get => _selectedFaculty;
        set
        {
            _selectedFaculty = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    public ObservableCollection<Teacher> Curators { get; set; } = new();
    private Teacher _selectedCurator;
    public Teacher SelectedCurator
    {
        get => _selectedCurator;
        set
        {
            _selectedCurator = value;
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
            UpdateGroupCommand.NotifyCanExecuteChanged();
            DeleteGroupCommand.NotifyCanExecuteChanged();
            LoadStudentsInGroupAsync();
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

    public RelayCommand LoadGroupsCommand { get; }
    public RelayCommand AddGroupCommand { get; }
    public RelayCommand UpdateGroupCommand { get; }
    public RelayCommand DeleteGroupCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public RelayCommand ExportToExcelCommand { get; }
    public RelayCommand ImportFromExcelCommand { get; }

    public GroupViewModel(
        GroupService groupService,
        FacultyService facultyService,
        TeacherService teacherService,
        StudentService studentService,
        string currentUserRole)
    {
        _groupService = groupService;
        _facultyService = facultyService;
        _teacherService = teacherService;
        _studentService = studentService;
        _currentUserRole = currentUserRole;

        LoadGroupsCommand = new RelayCommand(async () => await LoadDataAsync());
        AddGroupCommand = new RelayCommand(AddGroup, CanExecuteAddGroup);
        UpdateGroupCommand = new RelayCommand(UpdateGroup, CanExecuteUpdateGroup);
        DeleteGroupCommand = new RelayCommand(async () => await DeleteGroupAsync(), CanExecuteDeleteGroup);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        ExportToExcelCommand = new RelayCommand(ExportToExcel, CanExecuteExport);
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
        AddGroupCommand.NotifyCanExecuteChanged();
        UpdateGroupCommand.NotifyCanExecuteChanged();
        DeleteGroupCommand.NotifyCanExecuteChanged();
        ExportToExcelCommand.NotifyCanExecuteChanged();
        ImportFromExcelCommand.NotifyCanExecuteChanged();
    }

    public async Task LoadDataAsync()
    {
        Groups.Clear();
        _allGroups = (await _groupService.GetAllGroupsAsync()).ToList();

        Faculties.Clear();
        var faculties = await _facultyService.GetAllFacultiesAsync();
        foreach (var f in faculties)
            Faculties.Add(f);

        Curators.Clear();
        var curators = await _teacherService.GetAllTeachersAsync();
        foreach (var c in curators)
            Curators.Add(c);

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allGroups.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(GroupNameFilter))
            filtered = filtered.Where(g => g.GroupName.Contains(GroupNameFilter, StringComparison.OrdinalIgnoreCase));

        if (SelectedFaculty != null)
            filtered = filtered.Where(g => g.FacultyID == SelectedFaculty.FacultyID);

        if (SelectedCurator != null)
            filtered = filtered.Where(g => g.CuratorID == SelectedCurator.TeacherID);

        Groups.Clear();
        foreach (var group in filtered)
            Groups.Add(group);
    }

    private void ClearFilters()
    {
        GroupNameFilter = string.Empty;
        SelectedFaculty = null;
        SelectedCurator = null;
    }

    private async void LoadStudentsInGroupAsync()
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

    private bool CanExecuteAddGroup() => CurrentUserRole == "admin";
    private bool CanExecuteUpdateGroup() => SelectedGroup != null && CurrentUserRole == "admin";
    private bool CanExecuteDeleteGroup()
    {
        return CurrentUserRole == "admin" &&
               (SelectedGroup != null || (SelectedGroups != null && SelectedGroups.Any()));
    }
    private bool CanExecuteExport() => true;
    private bool CanExecuteImport() => CurrentUserRole == "admin";

    private async void AddGroup()
    {
        _isEditing = true;

        try
        {
            var newGroup = new Group();
            if (OpenGroupDialog(newGroup))
            {
                await _groupService.AddGroupAsync(newGroup);
                await LoadDataAsync();
            }
        }
        finally
        {
            _isEditing = false;
        }
    }


    private async void UpdateGroup()
    {
        if (SelectedGroup == null)
            return;

        _isEditing = true;

        try
        {
            if (OpenGroupDialog(SelectedGroup))
            {
                await _groupService.UpdateGroupAsync(SelectedGroup);
                await LoadDataAsync();
            }
        }
        finally
        {
            _isEditing = false;
        }
    }

    private async Task DeleteGroupAsync()
    {
        _isEditing = true;

        try
        {
            if (SelectedGroups != null && SelectedGroups.Any())
            {
                var idsToDelete = SelectedGroups.Select(g => g.GroupID).ToList();
                foreach (var id in idsToDelete)
                {
                    await _groupService.DeleteGroupAsync(id);
                }
            }
            else if (SelectedGroup != null)
            {
                await _groupService.DeleteGroupAsync(SelectedGroup.GroupID);
            }

            await LoadDataAsync();
        }
        finally
        {
            _isEditing = false;
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

    private void ExportToExcel()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = "Groups_Report.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Групи");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Назва групи";
            worksheet.Cell(1, 3).Value = "Факультет";
            worksheet.Cell(1, 4).Value = "Куратор";

            for (int i = 0; i < Groups.Count; i++)
            {
                var g = Groups[i];
                worksheet.Cell(i + 2, 1).Value = g.GroupID;
                worksheet.Cell(i + 2, 2).Value = g.GroupName;
                worksheet.Cell(i + 2, 3).Value = g.Faculty?.FacultyName ?? "";
                worksheet.Cell(i + 2, 4).Value = g.Curator?.FullName ?? "";
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

                    var existingGroups = await _groupService.GetAllGroupsAsync();
                    var existingFaculties = await _facultyService.GetAllFacultiesAsync();
                    var existingTeachers = await _teacherService.GetAllTeachersAsync();

                    int importedCount = 0;
                    int duplicateCount = 0;

                    foreach (var row in rows)
                    {
                        var groupName = row.Cell(1).GetString().Trim();
                        var facultyName = row.Cell(2).GetString().Trim();
                        var curatorName = row.Cell(3).GetString().Trim();

                        bool exists = existingGroups.Any(g =>
                            g.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));

                        if (!exists)
                        {
                            var faculty = existingFaculties.FirstOrDefault(f =>
                                f.FacultyName.Equals(facultyName, StringComparison.OrdinalIgnoreCase));

                            var curator = existingTeachers.FirstOrDefault(t =>
                                t.FullName.Equals(curatorName, StringComparison.OrdinalIgnoreCase));

                            var group = new Group
                            {
                                GroupName = groupName,
                                FacultyID = faculty?.FacultyID ?? 0,
                                CuratorID = curator?.TeacherID
                            };
                            await _groupService.AddGroupAsync(group);
                            importedCount++;
                        }
                        else
                        {
                            duplicateCount++;
                        }
                    }

                    await LoadDataAsync();
                    ShowMessage($"Імпорт завершено. Додано: {importedCount}, пропущено дублікати: {duplicateCount}");
                }
                catch (Exception ex)
                {
                    ShowMessage("Помилка імпорту: " + ex.Message);
                }
            }
        }
        finally
        {
            _isEditing = false;
        }
    }


    private void ShowMessage(string message)
    {
        System.Windows.MessageBox.Show(message);
    }
}
