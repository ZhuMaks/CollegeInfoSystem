using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CollegeInfoSystem.ViewModels;

public class TeacherViewModel : BaseViewModel
{
    private readonly TeacherService _teacherService;
    private Teacher _selectedTeacher;

    public ObservableCollection<Teacher> Teachers { get; set; } = new();

    public Teacher SelectedTeacher
    {
        get => _selectedTeacher;
        set
        {
            _selectedTeacher = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadTeachersCommand { get; }
    public ICommand AddTeacherCommand { get; }
    public ICommand UpdateTeacherCommand { get; }
    public ICommand DeleteTeacherCommand { get; }

    public TeacherViewModel(TeacherService teacherService)
    {
        _teacherService = teacherService;
        LoadTeachersCommand = new RelayCommand(async () => await LoadTeachersAsync());
        AddTeacherCommand = new RelayCommand(async () => await AddTeacherAsync());
        UpdateTeacherCommand = new RelayCommand(async () => await UpdateTeacherAsync(), () => SelectedTeacher != null);
        DeleteTeacherCommand = new RelayCommand(async () => await DeleteTeacherAsync(), () => SelectedTeacher != null);
    }

    public async Task LoadTeachersAsync()
    {
        Teachers.Clear();
        var teachers = await _teacherService.GetAllTeachersAsync();
        foreach (var teacher in teachers)
        {
            Teachers.Add(teacher);
        }
    }

    private async Task AddTeacherAsync()
    {
        var newTeacher = new Teacher { FirstName = "Новий", LastName = "Викладач", Email = "new@teacher.com", IsCurator = false };
        await _teacherService.AddTeacherAsync(newTeacher);
        await LoadTeachersAsync();
    }

    private async Task UpdateTeacherAsync()
    {
        if (SelectedTeacher != null)
        {
            await _teacherService.UpdateTeacherAsync(SelectedTeacher);
            await LoadTeachersAsync();
        }
    }

    private async Task DeleteTeacherAsync()
    {
        if (SelectedTeacher != null)
        {
            await _teacherService.DeleteTeacherAsync(SelectedTeacher.TeacherID);
            await LoadTeachersAsync();
        }
    }
}
