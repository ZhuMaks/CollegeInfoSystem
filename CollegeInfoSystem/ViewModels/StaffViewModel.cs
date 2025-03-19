using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

public class StaffViewModel : BaseViewModel, ILoadable
{
    private readonly StaffService _staffService;

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

    public RelayCommand LoadStaffCommand { get; }
    public RelayCommand AddStaffCommand { get; }
    public RelayCommand UpdateStaffCommand { get; }
    public RelayCommand DeleteStaffCommand { get; }

    public StaffViewModel(StaffService staffService)
    {
        _staffService = staffService;
        LoadStaffCommand = new RelayCommand(async () => await LoadDataAsync());
        AddStaffCommand = new RelayCommand(AddStaff);
        UpdateStaffCommand = new RelayCommand(UpdateStaff, () => SelectedStaff != null);
        DeleteStaffCommand = new RelayCommand(async () => await DeleteStaffAsync(), () => SelectedStaff != null);

        Task.Run(async () => await LoadDataAsync());
    }

    public async Task LoadDataAsync()
    {
        StaffList.Clear();
        var staffMembers = await _staffService.GetAllStaffAsync();
        foreach (var staff in staffMembers)
        {
            StaffList.Add(staff);
        }
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
