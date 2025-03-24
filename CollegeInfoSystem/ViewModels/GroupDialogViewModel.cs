using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

public class GroupDialogViewModel : BaseViewModel
{
    private Group _group;
    private Group _originalGroup;

    private readonly FacultyService _facultyService;
    private readonly TeacherService _teacherService;

    public Action CloseAction { get; set; }
    public bool IsSaved { get; private set; } = false;

    public ObservableCollection<Faculty> Faculties { get; set; } = new();
    public ObservableCollection<Teacher> Teachers { get; set; } = new();

    public string GroupName
    {
        get => _group.GroupName;
        set
        {
            _group.GroupName = value;
            OnPropertyChanged();
        }
    }

    public Faculty SelectedFaculty
    {
        get => _group.Faculty;
        set
        {
            _group.Faculty = value;
            OnPropertyChanged();
        }
    }

    public Teacher SelectedCurator
    {
        get => _group.Curator;
        set
        {
            _group.Curator = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public GroupDialogViewModel(Group group, FacultyService facultyService, TeacherService teacherService)
    {
        _group = group;
        _originalGroup = new Group
        {
            GroupName = group.GroupName,
            Faculty = group.Faculty,
            Curator = group.Curator
        };

        _facultyService = facultyService;
        _teacherService = teacherService;

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);

        Task.Run(async () => await LoadOptionsAsync());
    }

    private async Task LoadOptionsAsync()
    {
        var faculties = await _facultyService.GetAllFacultiesAsync();
        var teachers = await _teacherService.GetAllTeachersAsync();

        Faculties = new ObservableCollection<Faculty>(faculties);
        Teachers = new ObservableCollection<Teacher>(teachers.Where(t => t.IsCurator)); // Фільтрація лише кураторів
    }


    private void Save()
    {
        IsSaved = true;
        CloseAction?.Invoke();
    }

    private void Cancel()
    {
        IsSaved = false;
        _group.GroupName = _originalGroup.GroupName;
        _group.Faculty = _originalGroup.Faculty;
        _group.Curator = _originalGroup.Curator;

        CloseAction?.Invoke();
    }
}
