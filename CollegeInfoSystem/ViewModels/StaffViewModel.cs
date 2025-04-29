using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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
            ((RelayCommand)UpdateStaffCommand).NotifyCanExecuteChanged();
            ((RelayCommand)DeleteStaffCommand).NotifyCanExecuteChanged();
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

    public RelayCommand LoadStaffCommand { get; }
    public RelayCommand AddStaffCommand { get; }
    public RelayCommand UpdateStaffCommand { get; }
    public RelayCommand DeleteStaffCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }

    public StaffViewModel(StaffService staffService)
    {
        _staffService = staffService;
        LoadStaffCommand = new RelayCommand(async () => await LoadDataAsync());
        AddStaffCommand = new RelayCommand(AddStaff);
        UpdateStaffCommand = new RelayCommand(UpdateStaff, () => SelectedStaff != null);
        DeleteStaffCommand = new RelayCommand(async () => await DeleteStaffAsync(), () => SelectedStaff != null);
        ClearFiltersCommand = new RelayCommand(ClearFilters);

        Task.Run(async () => await LoadDataAsync());
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
            filtered = filtered.Where(s => s.LastName.Contains(LastNameFilter, System.StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(PositionFilter))
            filtered = filtered.Where(s => s.Position.Contains(PositionFilter, System.StringComparison.OrdinalIgnoreCase));

        StaffList.Clear();
        foreach (var staff in filtered)
        {
            StaffList.Add(staff);
        }
    }

    private void ClearFilters()
    {
        LastNameFilter = string.Empty;
        PositionFilter = string.Empty;
    }

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
        if (SelectedStaff != null)
        {
            OpenStaffDialog(SelectedStaff);
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
}
