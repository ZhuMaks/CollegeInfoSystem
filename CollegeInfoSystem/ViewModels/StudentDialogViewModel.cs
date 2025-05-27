using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

public class StudentDialogViewModel : BaseViewModel
{
    private Student _student;
    private Student _originalStudent;
    private readonly GroupService _groupService;

    public Action CloseAction { get; set; }
    public bool IsSaved { get; private set; } = false;

    public ObservableCollection<Group> Groups { get; set; } = new();

    public string FirstName
    {
        get => _student.FirstName;
        set
        {
            _student.FirstName = value;
            OnPropertyChanged();
        }
    }

    public string LastName
    {
        get => _student.LastName;
        set
        {
            _student.LastName = value;
            OnPropertyChanged();
        }
    }

    public string Email
    {
        get => _student.Email;
        set
        {
            _student.Email = value;
            OnPropertyChanged();
        }
    }

    public string Phone
    {
        get => _student.Phone;
        set
        {
            _student.Phone = value;
            OnPropertyChanged();
        }
    }

    public DateTime DateOfBirth
    {
        get => _student.DateOfBirth;
        set
        {
            _student.DateOfBirth = value;
            OnPropertyChanged();
        }
    }

    public string Address
    {
        get => _student.Address;
        set
        {
            _student.Address = value;
            OnPropertyChanged();
        }
    }

    public Group SelectedGroup
    {
        get => _student.Group;
        set
        {
            _student.Group = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public StudentDialogViewModel(Student student, GroupService groupService)
    {
        _student = student;
        _originalStudent = new Student
        {
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            Phone = student.Phone,
            DateOfBirth = student.DateOfBirth,
            Address = student.Address,
            Group = student.Group
        };

        _groupService = groupService;

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);

        Task.Run(async () => await LoadGroupsAsync());
    }

    private async Task LoadGroupsAsync()
    {
        var groups = await _groupService.GetAllGroupsAsync();
        Application.Current.Dispatcher.Invoke(() =>
        {
            Groups.Clear();
            foreach (var group in groups)
                Groups.Add(group);
        });
    }


    private bool ValidateFields()
    {
        return !string.IsNullOrWhiteSpace(FirstName) &&
               !string.IsNullOrWhiteSpace(LastName) &&
               !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(Phone) &&
               !string.IsNullOrWhiteSpace(Address) &&
               SelectedGroup != null;
    }

    private void Save()
    {
        if (!ValidateFields())
        {
            MessageBox.Show("Усі поля мають бути заповнені!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsSaved = true;
        CloseAction?.Invoke();
    }

    private void Cancel()
    {
        IsSaved = false;
        _student.FirstName = _originalStudent.FirstName;
        _student.LastName = _originalStudent.LastName;
        _student.Email = _originalStudent.Email;
        _student.Phone = _originalStudent.Phone;
        _student.DateOfBirth = _originalStudent.DateOfBirth;
        _student.Address = _originalStudent.Address;
        _student.Group = _originalStudent.Group;

        CloseAction?.Invoke();
    }
}
