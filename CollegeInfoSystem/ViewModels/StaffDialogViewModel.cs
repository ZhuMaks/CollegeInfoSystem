using CollegeInfoSystem.Models;
using CollegeInfoSystem.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

public class StaffDialogViewModel : BaseViewModel
{
    private Staff _staff;
    private Staff _originalStaff;

    public Action CloseAction { get; set; }
    public bool IsSaved { get; private set; } = false;

    public Staff Staff
    {
        get => _staff;
        set
        {
            _staff = value;
            OnPropertyChanged();
        }
    }

    public string FirstName
    {
        get => _staff.FirstName;
        set
        {
            _staff.FirstName = value;
            OnPropertyChanged();
        }
    }

    public string LastName
    {
        get => _staff.LastName;
        set
        {
            _staff.LastName = value;
            OnPropertyChanged();
        }
    }

    public string Position
    {
        get => _staff.Position;
        set
        {
            _staff.Position = value;
            OnPropertyChanged();
        }
    }

    public string Email
    {
        get => _staff.Email;
        set
        {
            _staff.Email = value;
            OnPropertyChanged();
        }
    }

    public string Phone
    {
        get => _staff.Phone;
        set
        {
            _staff.Phone = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public StaffDialogViewModel(Staff staff)
    {
        _staff = staff;
        _originalStaff = new Staff
        {
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            Position = staff.Position,
            Email = staff.Email,
            Phone = staff.Phone
        };

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
    }

    private bool ValidateFields()
    {
        return !string.IsNullOrWhiteSpace(FirstName) &&
               !string.IsNullOrWhiteSpace(LastName) &&
               !string.IsNullOrWhiteSpace(Position) &&
               !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(Phone);
    }

    public void Save()
    {
        if (!ValidateFields())
        {
            MessageBox.Show("Всі поля повинні бути заповнені!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsSaved = true;
        CloseAction?.Invoke();
    }

    public void Cancel()
    {
        IsSaved = false;

        _staff.FirstName = _originalStaff.FirstName;
        _staff.LastName = _originalStaff.LastName;
        _staff.Position = _originalStaff.Position;
        _staff.Email = _originalStaff.Email;
        _staff.Phone = _originalStaff.Phone;

        CloseAction?.Invoke();
    }
}
