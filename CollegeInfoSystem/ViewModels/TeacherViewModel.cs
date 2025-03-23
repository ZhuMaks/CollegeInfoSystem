using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public class TeacherViewModel : BaseViewModel, ILoadable
{
    private readonly TeacherService _teacherService;

    public ObservableCollection<Teacher> Teachers { get; set; } = new();

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

    public RelayCommand LoadTeachersCommand { get; }
    public RelayCommand AddTeacherCommand { get; }
    public RelayCommand UpdateTeacherCommand { get; }
    public RelayCommand DeleteTeacherCommand { get; }

    public TeacherViewModel(TeacherService teacherService)
    {
        _teacherService = teacherService;
        LoadTeachersCommand = new RelayCommand(async () => await LoadDataAsync());
        AddTeacherCommand = new RelayCommand(AddTeacher);
        UpdateTeacherCommand = new RelayCommand(UpdateTeacher, () => SelectedTeacher != null);
        DeleteTeacherCommand = new RelayCommand(async () => await DeleteTeacherAsync(), () => SelectedTeacher != null);

        Task.Run(async () => await LoadDataAsync());
    }

    public async Task LoadDataAsync()
    {
        Teachers.Clear();
        var teachers = await _teacherService.GetAllTeachersAsync();
        foreach (var teacher in teachers)
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
}
