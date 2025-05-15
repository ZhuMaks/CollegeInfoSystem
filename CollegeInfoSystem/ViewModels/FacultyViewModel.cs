using ClosedXML.Excel;
using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace CollegeInfoSystem.ViewModels;

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
    public RelayCommand ImportFromExcelCommand { get; }

    public FacultyViewModel(FacultyService facultyService)
    {
        _facultyService = facultyService;
        LoadFacultiesCommand = new RelayCommand(async () => await LoadDataAsync());
        AddFacultyCommand = new RelayCommand(AddFaculty);
        UpdateFacultyCommand = new RelayCommand(UpdateFaculty, () => SelectedFaculty != null);
        DeleteFacultyCommand = new RelayCommand(async () => await DeleteFacultyAsync(), () => SelectedFaculty != null);
        ImportFromExcelCommand = new RelayCommand(async () => await ImportFromExcel());

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
        if (OpenFacultyDialog(newFaculty))
        {
            await _facultyService.AddFacultyAsync(newFaculty);
            await LoadDataAsync();
        }
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
