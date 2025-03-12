using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CollegeInfoSystem.ViewModels;

public class StudentViewModel : BaseViewModel
{
    private readonly StudentService _studentService;
    private Student _selectedStudent;

    public ObservableCollection<Student> Students { get; set; } = new();

    public Student SelectedStudent
    {
        get => _selectedStudent;
        set
        {
            _selectedStudent = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadStudentsCommand { get; }
    public ICommand AddStudentCommand { get; }
    public ICommand UpdateStudentCommand { get; }
    public ICommand DeleteStudentCommand { get; }

    public StudentViewModel(StudentService studentService)
    {
        _studentService = studentService;
        LoadStudentsCommand = new RelayCommand(async () => await LoadStudentsAsync());
        AddStudentCommand = new RelayCommand(async () => await AddStudentAsync());
        UpdateStudentCommand = new RelayCommand(async () => await UpdateStudentAsync(), () => SelectedStudent != null);
        DeleteStudentCommand = new RelayCommand(async () => await DeleteStudentAsync(), () => SelectedStudent != null);
    }

    public async Task LoadStudentsAsync()
    {
        Students.Clear();
        var students = await _studentService.GetAllStudentsAsync();
        foreach (var student in students)
        {
            Students.Add(student);
        }
    }

    private async Task AddStudentAsync()
    {
        var newStudent = new Student { FirstName = "Новий", LastName = "Студент", GroupID = 1, Email = "new@email.com" };
        await _studentService.AddStudentAsync(newStudent);
        await LoadStudentsAsync();
    }

    private async Task UpdateStudentAsync()
    {
        if (SelectedStudent != null)
        {
            await _studentService.UpdateStudentAsync(SelectedStudent);
            await LoadStudentsAsync();
        }
    }

    private async Task DeleteStudentAsync()
    {
        if (SelectedStudent != null)
        {
            await _studentService.DeleteStudentAsync(SelectedStudent.StudentID);
            await LoadStudentsAsync();
        }
    }
}
