using ClosedXML.Excel;
using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeInfoSystem.ViewModels;
public class StaffViewModel : BaseViewModel, ILoadable
{
    private readonly StaffService _staffService;
    private List<Staff> _allStaff;

    public ObservableCollection<Staff> StaffList { get; set; } = new();

    private Staff _selectedStaff;
    public Staff SelectedStaff
    {
        get => _selectedStaff;
        set
        {
            _selectedStaff = value;
            OnPropertyChanged();
            UpdateStaffCommand.NotifyCanExecuteChanged();
            DeleteStaffCommand.NotifyCanExecuteChanged();
        }
    }

    private string _lastNameFilter;
    public string LastNameFilter
    {
        get => _lastNameFilter;
        set
        {
            _lastNameFilter = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    private string _positionFilter;
    public string PositionFilter
    {
        get => _positionFilter;
        set
        {
            _positionFilter = value;
            OnPropertyChanged();
            ApplyFilters();
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
            UpdateCommandStates();
        }
    }

    public RelayCommand LoadStaffCommand { get; }
    public RelayCommand AddStaffCommand { get; }
    public RelayCommand UpdateStaffCommand { get; }
    public RelayCommand DeleteStaffCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public RelayCommand ExportToExcelCommand { get; }
    public RelayCommand ImportFromExcelCommand { get; }

    public StaffViewModel(StaffService staffService, string currentUserRole)
    {
        _staffService = staffService;
        _currentUserRole = currentUserRole;

        LoadStaffCommand = new RelayCommand(async () => await LoadDataAsync());
        AddStaffCommand = new RelayCommand(AddStaff, CanExecuteAddOrEdit);
        UpdateStaffCommand = new RelayCommand(UpdateStaff, CanExecuteEdit);
        DeleteStaffCommand = new RelayCommand(async () => await DeleteStaffAsync(), CanExecuteDelete);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        ExportToExcelCommand = new RelayCommand(ExportToExcel, CanExecuteExport);
        ImportFromExcelCommand = new RelayCommand(async () => await ImportFromExcel(), CanExecuteImport);

        Task.Run(async () => await LoadDataAsync());
    }

    private void UpdateCommandStates()
    {
        AddStaffCommand.NotifyCanExecuteChanged();
        UpdateStaffCommand.NotifyCanExecuteChanged();
        DeleteStaffCommand.NotifyCanExecuteChanged();
        ExportToExcelCommand.NotifyCanExecuteChanged();
        ImportFromExcelCommand.NotifyCanExecuteChanged();
    }

    public async Task LoadDataAsync()
    {
        _allStaff = (await _staffService.GetAllStaffAsync()).ToList();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allStaff.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(LastNameFilter))
            filtered = filtered.Where(s => s.LastName.Contains(LastNameFilter, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(PositionFilter))
            filtered = filtered.Where(s => s.Position.Contains(PositionFilter, StringComparison.OrdinalIgnoreCase));

        StaffList.Clear();
        foreach (var staff in filtered)
            StaffList.Add(staff);
    }

    private void ClearFilters()
    {
        LastNameFilter = string.Empty;
        PositionFilter = string.Empty;
        ApplyFilters();
    }

    private bool CanExecuteAddOrEdit() => CurrentUserRole == "admin";
    private bool CanExecuteEdit() => SelectedStaff != null && CurrentUserRole == "admin";
    private bool CanExecuteDelete() => SelectedStaff != null && CurrentUserRole == "admin";
    private bool CanExecuteExport() => true; // всім доступно
    private bool CanExecuteImport() => CurrentUserRole == "admin";

    private async void AddStaff()
    {
        var newStaff = new Staff();
        if (OpenStaffDialog(newStaff))
        {
            await _staffService.AddStaffAsync(newStaff);
            await LoadDataAsync();
        }
    }

    private async void UpdateStaff()
    {
        if (SelectedStaff != null && OpenStaffDialog(SelectedStaff))
        {
            await _staffService.UpdateStaffAsync(SelectedStaff);
            await LoadDataAsync();
        }
    }

    private async Task DeleteStaffAsync()
    {
        if (SelectedStaff != null)
        {
            await _staffService.DeleteStaffAsync(SelectedStaff.StaffID);
            await LoadDataAsync();
        }
    }

    private bool OpenStaffDialog(Staff staff)
    {
        var viewModel = new StaffDialogViewModel(staff);
        var dialog = new StaffDialog { DataContext = viewModel };

        bool isSaved = false;
        viewModel.CloseAction = () =>
        {
            isSaved = viewModel.IsSaved;
            dialog.Close();
        };

        dialog.ShowDialog();
        return isSaved;
    }

    private void ExportToExcel()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = "Staff_Report.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Працівники");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Ім'я";
            worksheet.Cell(1, 3).Value = "Прізвище";
            worksheet.Cell(1, 4).Value = "Посада";
            worksheet.Cell(1, 5).Value = "Email";
            worksheet.Cell(1, 6).Value = "Телефон";

            for (int i = 0; i < StaffList.Count; i++)
            {
                var s = StaffList[i];
                worksheet.Cell(i + 2, 1).Value = s.StaffID;
                worksheet.Cell(i + 2, 2).Value = s.FirstName;
                worksheet.Cell(i + 2, 3).Value = s.LastName;
                worksheet.Cell(i + 2, 4).Value = s.Position;
                worksheet.Cell(i + 2, 5).Value = s.Email;
                worksheet.Cell(i + 2, 6).Value = s.Phone;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);
        }
    }

    private async Task ImportFromExcel()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx",
            Title = "Виберіть файл Excel"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                using var workbook = new XLWorkbook(dialog.FileName);
                var worksheet = workbook.Worksheets.First();
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                var existingStaff = await _staffService.GetAllStaffAsync();

                int imported = 0, duplicates = 0;

                foreach (var row in rows)
                {
                    var firstName = row.Cell(1).GetString().Trim();
                    var lastName = row.Cell(2).GetString().Trim();
                    var position = row.Cell(3).GetString().Trim();
                    var email = row.Cell(4).GetString().Trim();
                    var phone = row.Cell(5).GetString().Trim();

                    bool exists = existingStaff.Any(s =>
                        s.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                        s.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase) &&
                        s.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                    if (!exists)
                    {
                        var staff = new Staff
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            Position = position,
                            Email = email,
                            Phone = phone
                        };

                        await _staffService.AddStaffAsync(staff);
                        imported++;
                    }
                    else
                    {
                        duplicates++;
                    }
                }

                await LoadDataAsync();

                System.Windows.MessageBox.Show($"Імпорт завершено:\nДодано: {imported}\nПропущено: {duplicates}", "Результат імпорту");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Помилка при імпорті: " + ex.Message);
            }
        }
    }
}
