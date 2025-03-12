using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CollegeInfoSystem.ViewModels;

public class StaffViewModel : BaseViewModel
{
    private readonly StaffService _staffService;
    private Staff _selectedStaff;

    public ObservableCollection<Staff> StaffList { get; set; } = new();

    public Staff SelectedStaff
    {
        get => _selectedStaff;
        set
        {
            _selectedStaff = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadStaffCommand { get; }
    public ICommand AddStaffCommand { get; }
    public ICommand UpdateStaffCommand { get; }
    public ICommand DeleteStaffCommand { get; }

    public StaffViewModel(StaffService staffService)
    {
        _staffService = staffService;
        LoadStaffCommand = new RelayCommand(async () => await LoadStaffAsync());
        AddStaffCommand = new RelayCommand(async () => await AddStaffAsync());
        UpdateStaffCommand = new RelayCommand(async () => await UpdateStaffAsync(), () => SelectedStaff != null);
        DeleteStaffCommand = new RelayCommand(async () => await DeleteStaffAsync(), () => SelectedStaff != null);
    }

    public async Task LoadStaffAsync()
    {
        StaffList.Clear();
        var staffMembers = await _staffService.GetAllStaffAsync();
        foreach (var staff in staffMembers)
        {
            StaffList.Add(staff);
        }
    }

    private async Task AddStaffAsync()
    {
        var newStaff = new Staff { FirstName = "Новий", LastName = "Працівник", Position = "Посада" };
        await _staffService.AddStaffAsync(newStaff);
        await LoadStaffAsync();
    }

    private async Task UpdateStaffAsync()
    {
        if (SelectedStaff != null)
        {
            await _staffService.UpdateStaffAsync(SelectedStaff);
            await LoadStaffAsync();
        }
    }

    private async Task DeleteStaffAsync()
    {
        if (SelectedStaff != null)
        {
            await _staffService.DeleteStaffAsync(SelectedStaff.StaffID);
            await LoadStaffAsync();
        }
    }
}
