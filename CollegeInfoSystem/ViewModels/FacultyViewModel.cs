using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public class FacultyViewModel : BaseViewModel, ILoadable
{
    private readonly FacultyService _facultyService;

    public ObservableCollection<Faculty> Faculties { get; set; } = new();

    private Faculty _selectedFaculty;
    public Faculty SelectedFaculty
    {
        get => _selectedFaculty;
        set
        {
            _selectedFaculty = value;
            OnPropertyChanged();
            ((RelayCommand)UpdateFacultyCommand).NotifyCanExecuteChanged();
            ((RelayCommand)DeleteFacultyCommand).NotifyCanExecuteChanged();
        }
    }

    public RelayCommand LoadFacultiesCommand { get; }
    public RelayCommand AddFacultyCommand { get; }
    public RelayCommand UpdateFacultyCommand { get; }
    public RelayCommand DeleteFacultyCommand { get; }

    public FacultyViewModel(FacultyService facultyService)
    {
        _facultyService = facultyService;
        LoadFacultiesCommand = new RelayCommand(async () => await LoadDataAsync());
        AddFacultyCommand = new RelayCommand(AddFaculty);
        UpdateFacultyCommand = new RelayCommand(UpdateFaculty, () => SelectedFaculty != null);
        DeleteFacultyCommand = new RelayCommand(async () => await DeleteFacultyAsync(), () => SelectedFaculty != null);

        Task.Run(async () => await LoadDataAsync());
    }

    public async Task LoadDataAsync()
    {
        Faculties.Clear();
        var faculties = await _facultyService.GetAllFacultiesAsync();
        foreach (var faculty in faculties)
        {
            Faculties.Add(faculty);
        }
    }

    private async void AddFaculty()
    {
        var newFaculty = new Faculty();
        OpenFacultyDialog(newFaculty);

        await _facultyService.AddFacultyAsync(newFaculty);
        await LoadDataAsync();
    }

    private async void UpdateFaculty()
    {
        if (SelectedFaculty != null)
        {
            OpenFacultyDialog(SelectedFaculty);

            await _facultyService.UpdateFacultyAsync(SelectedFaculty);
            await LoadDataAsync();
        }
    }

    private async Task DeleteFacultyAsync()
    {
        if (SelectedFaculty != null)
        {
            await _facultyService.DeleteFacultyAsync(SelectedFaculty.FacultyID);
            await LoadDataAsync();
        }
    }

    private void OpenFacultyDialog(Faculty faculty)
    {
        var viewModel = new FacultyDialogViewModel(faculty);
        var dialog = new FacultyDialog { DataContext = viewModel };

        viewModel.CloseAction = () => dialog.Close();
        dialog.ShowDialog();
    }
}
