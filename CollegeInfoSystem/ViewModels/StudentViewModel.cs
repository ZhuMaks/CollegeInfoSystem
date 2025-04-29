using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;

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

    public StudentViewModel(StudentService studentService, GroupService groupService)
    {
        _studentService = studentService;
        _groupService = groupService;

        LoadStudentsCommand = new RelayCommand(async () => await LoadDataAsync());
        AddStudentCommand = new RelayCommand(AddStudent);
        UpdateStudentCommand = new RelayCommand(UpdateStudent, () => SelectedStudent != null);
        DeleteStudentCommand = new RelayCommand(async () => await DeleteStudentAsync(), () => SelectedStudent != null);
        ClearFiltersCommand = new RelayCommand(ClearFilters);

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
}
