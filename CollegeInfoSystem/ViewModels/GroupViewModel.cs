using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public class GroupViewModel : BaseViewModel, ILoadable
{
    private readonly GroupService _groupService;
    private readonly FacultyService _facultyService;
    private readonly TeacherService _teacherService;
    private readonly StudentService _studentService;

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

    public RelayCommand LoadGroupsCommand { get; }
    public RelayCommand AddGroupCommand { get; }
    public RelayCommand UpdateGroupCommand { get; }
    public RelayCommand DeleteGroupCommand { get; }

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

        Task.Run(async () => await LoadDataAsync());
    }

    public async Task LoadDataAsync()
    {
        Groups.Clear();
        var groups = await _groupService.GetAllGroupsAsync();
        foreach (var group in groups)
        {
            Groups.Add(group);
        }
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
}
