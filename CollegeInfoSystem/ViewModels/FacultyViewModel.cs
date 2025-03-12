using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CollegeInfoSystem.ViewModels;

public class FacultyViewModel : BaseViewModel
{
    private readonly FacultyService _facultyService;
    private Faculty _selectedFaculty;

    public ObservableCollection<Faculty> Faculties { get; set; } = new();

    public Faculty SelectedFaculty
    {
        get => _selectedFaculty;
        set
        {
            _selectedFaculty = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadFacultiesCommand { get; }
    public ICommand AddFacultyCommand { get; }
    public ICommand UpdateFacultyCommand { get; }
    public ICommand DeleteFacultyCommand { get; }

    public FacultyViewModel(FacultyService facultyService)
    {
        _facultyService = facultyService;
        LoadFacultiesCommand = new RelayCommand(async () => await LoadFacultiesAsync());
        AddFacultyCommand = new RelayCommand(async () => await AddFacultyAsync());
        UpdateFacultyCommand = new RelayCommand(async () => await UpdateFacultyAsync(), () => SelectedFaculty != null);
        DeleteFacultyCommand = new RelayCommand(async () => await DeleteFacultyAsync(), () => SelectedFaculty != null);
    }

    public async Task LoadFacultiesAsync()
    {
        Faculties.Clear();
        var faculties = await _facultyService.GetAllFacultiesAsync();
        foreach (var faculty in faculties)
        {
            Faculties.Add(faculty);
        }
    }

    private async Task AddFacultyAsync()
    {
        var newFaculty = new Faculty { FacultyName = "Новий факультет" };
        await _facultyService.AddFacultyAsync(newFaculty);
        await LoadFacultiesAsync();
    }

    private async Task UpdateFacultyAsync()
    {
        if (SelectedFaculty != null)
        {
            await _facultyService.UpdateFacultyAsync(SelectedFaculty);
            await LoadFacultiesAsync();
        }
    }

    private async Task DeleteFacultyAsync()
    {
        if (SelectedFaculty != null)
        {
            await _facultyService.DeleteFacultyAsync(SelectedFaculty.FacultyID);
            await LoadFacultiesAsync();
        }
    }
}
