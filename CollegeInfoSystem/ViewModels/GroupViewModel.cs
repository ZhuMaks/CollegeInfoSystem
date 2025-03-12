using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CollegeInfoSystem.ViewModels;

public class GroupViewModel : BaseViewModel
{
    private readonly GroupService _groupService;
    private Group _selectedGroup;
    private ObservableCollection<Student> _studentsInGroup = new();

    public ObservableCollection<Group> Groups { get; set; } = new();
    public ObservableCollection<Student> StudentsInGroup
    {
        get => _studentsInGroup;
        set
        {
            _studentsInGroup = value;
            OnPropertyChanged();
        }
    }

    public Group SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            _selectedGroup = value;
            OnPropertyChanged();
            LoadStudentsAsync();
        }
    }

    public ICommand LoadGroupsCommand { get; }
    public ICommand AddGroupCommand { get; }
    public ICommand UpdateGroupCommand { get; }
    public ICommand DeleteGroupCommand { get; }

    public GroupViewModel(GroupService groupService)
    {
        _groupService = groupService;
        LoadGroupsCommand = new RelayCommand(async () => await LoadGroupsAsync());
        AddGroupCommand = new RelayCommand(async () => await AddGroupAsync());
        UpdateGroupCommand = new RelayCommand(async () => await UpdateGroupAsync(), () => SelectedGroup != null);
        DeleteGroupCommand = new RelayCommand(async () => await DeleteGroupAsync(), () => SelectedGroup != null);
    }

    public async Task LoadGroupsAsync()
    {
        Groups.Clear();
        var groups = await _groupService.GetAllGroupsAsync();
        foreach (var group in groups)
        {
            Groups.Add(group);
        }
    }

    private async Task AddGroupAsync()
    {
        var newGroup = new Group { GroupName = "Нова група", FacultyID = 1, CuratorID = null };
        await _groupService.AddGroupAsync(newGroup);
        await LoadGroupsAsync();
    }

    private async Task UpdateGroupAsync()
    {
        if (SelectedGroup != null)
        {
            await _groupService.UpdateGroupAsync(SelectedGroup);
            await LoadGroupsAsync();
        }
    }

    private async Task DeleteGroupAsync()
    {
        if (SelectedGroup != null)
        {
            await _groupService.DeleteGroupAsync(SelectedGroup.GroupID);
            await LoadGroupsAsync();
        }
    }

    private async Task LoadStudentsAsync()
    {
        if (SelectedGroup != null)
        {
            var students = await _groupService.GetStudentsByGroupAsync(SelectedGroup.GroupID);
            StudentsInGroup = new ObservableCollection<Student>(students);
        }
    }
}
