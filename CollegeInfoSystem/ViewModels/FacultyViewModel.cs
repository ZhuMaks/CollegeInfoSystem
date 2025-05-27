using ClosedXML.Excel;
using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CollegeInfoSystem.ViewModels;

public class FacultyViewModel : BaseViewModel, ILoadable
{
    private readonly FacultyService _facultyService;

    public ObservableCollection<Faculty> Faculties { get; set; } = new();
    public ObservableCollection<Faculty> SelectedFaculties { get; set; } = new();

    private DispatcherTimer _refreshTimer;
    private Faculty _selectedFaculty;
    public Faculty SelectedFaculty
    {
        get => _selectedFaculty;
        set
        {
            _selectedFaculty = value;
            OnPropertyChanged();
            UpdateFacultyCommand.NotifyCanExecuteChanged();
            DeleteFacultyCommand.NotifyCanExecuteChanged();
        }
    }

    private string _currentUserRole;
    public string CurrentUserRole
    {
        get => _currentUserRole;
        set
        {
            _currentUserRole = value;
            OnPropertyChanged();
            UpdateCommandCanExecute();
        }
    }

    public RelayCommand LoadFacultiesCommand { get; }
    public RelayCommand AddFacultyCommand { get; }
    public RelayCommand UpdateFacultyCommand { get; }
    public RelayCommand DeleteFacultyCommand { get; }
    public RelayCommand ImportFromExcelCommand { get; }

    public FacultyViewModel(FacultyService facultyService, string currentUserRole)
    {
        _facultyService = facultyService;
        _currentUserRole = currentUserRole;

        LoadFacultiesCommand = new RelayCommand(async () => await LoadDataAsync());
        AddFacultyCommand = new RelayCommand(AddFaculty, CanExecuteAddFaculty);
        UpdateFacultyCommand = new RelayCommand(UpdateFaculty, CanExecuteUpdateFaculty);
        DeleteFacultyCommand = new RelayCommand(async () => await DeleteFacultyAsync(), CanExecuteDeleteFaculty);
        ImportFromExcelCommand = new RelayCommand(async () => await ImportFromExcel(), CanExecuteImport);

        Task.Run(async () => await LoadDataAsync());

        _refreshTimer = new DispatcherTimer();
        _refreshTimer.Interval = TimeSpan.FromSeconds(15);
        _refreshTimer.Tick += RefreshTimer_Tick;
        _refreshTimer.Start();
    }
    private async void RefreshTimer_Tick(object? sender, EventArgs e)
    {
        _refreshTimer.Stop(); 
        try
        {
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Помилка при оновленні: " + ex.Message);
        }
        finally
        {
            _refreshTimer.Start(); 
        }
    }

    private void UpdateCommandCanExecute()
    {
        AddFacultyCommand.NotifyCanExecuteChanged();
        UpdateFacultyCommand.NotifyCanExecuteChanged();
        DeleteFacultyCommand.NotifyCanExecuteChanged();
        ImportFromExcelCommand.NotifyCanExecuteChanged();
    }

    private bool CanExecuteAddFaculty() => CurrentUserRole == "admin";
    private bool CanExecuteUpdateFaculty() => SelectedFaculty != null && CurrentUserRole == "admin";
    private bool CanExecuteDeleteFaculty()
    {
        return (SelectedFaculty != null || SelectedFaculties.Any()) && CurrentUserRole == "admin";
    }
    private bool CanExecuteImport() => CurrentUserRole == "admin";

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
        if (OpenFacultyDialog(newFaculty))
        {
            await _facultyService.AddFacultyAsync(newFaculty);
            await LoadDataAsync();
        }
    }

    private async void UpdateFaculty()
    {
        if (SelectedFaculty != null && OpenFacultyDialog(SelectedFaculty))
        {
            await _facultyService.UpdateFacultyAsync(SelectedFaculty);
            await LoadDataAsync();
        }
    }

    private async Task DeleteFacultyAsync()
    {
        if (SelectedFaculties?.Count > 1)
        {
            var idsToDelete = SelectedFaculties.Select(f => f.FacultyID).ToList();
            foreach (var id in idsToDelete)
                await _facultyService.DeleteFacultyAsync(id);
        }
        else if (SelectedFaculty != null)
        {
            await _facultyService.DeleteFacultyAsync(SelectedFaculty.FacultyID);
        }

        await LoadDataAsync();
    }


    private bool OpenFacultyDialog(Faculty faculty)
    {
        var viewModel = new FacultyDialogViewModel(faculty);
        var dialog = new FacultyDialog { DataContext = viewModel };

        bool isSaved = false;
        viewModel.CloseAction = () =>
        {
            isSaved = viewModel.IsSaved;
            dialog.Close();
        };

        dialog.ShowDialog();
        return isSaved;
    }

    private async Task ImportFromExcel()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx",
            Title = "Виберіть Excel-файл з факультетами"
        };

        if (dialog.ShowDialog() == true)
        {
            using var workbook = new XLWorkbook(dialog.FileName);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); 

            var existingFaculties = await _facultyService.GetAllFacultiesAsync();

            int importedCount = 0;
            int duplicateCount = 0;

            foreach (var row in rows)
            {
                var facultyName = row.Cell(1).GetString().Trim();

                bool exists = existingFaculties.Any(f =>
                    f.FacultyName.Equals(facultyName, System.StringComparison.OrdinalIgnoreCase));

                if (!exists)
                {
                    var faculty = new Faculty
                    {
                        FacultyName = facultyName
                    };

                    await _facultyService.AddFacultyAsync(faculty);
                    importedCount++;
                }
                else
                {
                    duplicateCount++;
                }
            }

            await LoadDataAsync();

            MessageBox.Show(
                $"Імпорт завершено:\nДодано: {importedCount}\nПропущено (дублікати): {duplicateCount}",
                "Результат імпорту",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}
