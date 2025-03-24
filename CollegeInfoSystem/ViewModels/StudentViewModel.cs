using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public class StudentViewModel : BaseViewModel, ILoadable
{
    private readonly StudentService _studentService;
    private readonly GroupService _groupService;

    public ObservableCollection<Student> Students { get; set; } = new();
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

    public RelayCommand LoadStudentsCommand { get; }
    public RelayCommand AddStudentCommand { get; }
    public RelayCommand UpdateStudentCommand { get; }
    public RelayCommand DeleteStudentCommand { get; }

    public StudentViewModel(StudentService studentService, GroupService groupService)
    {
        _studentService = studentService;
        _groupService = groupService;

        LoadStudentsCommand = new RelayCommand(async () => await LoadDataAsync());
        AddStudentCommand = new RelayCommand(AddStudent);
        UpdateStudentCommand = new RelayCommand(UpdateStudent, () => SelectedStudent != null);
        DeleteStudentCommand = new RelayCommand(async () => await DeleteStudentAsync(), () => SelectedStudent != null);

        Task.Run(async () => await LoadDataAsync());
    }

    public async Task LoadDataAsync()
    {
        Students.Clear();
        var students = await _studentService.GetAllStudentsAsync();
        foreach (var student in students)
        {
            Students.Add(student);
        }
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
