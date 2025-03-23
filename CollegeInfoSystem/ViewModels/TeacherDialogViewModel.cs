using CollegeInfoSystem.Models;
using CollegeInfoSystem.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;

public class TeacherDialogViewModel : BaseViewModel
{
    private Teacher _teacher;
    private Teacher _originalTeacher;

    public Action CloseAction { get; set; }
    public bool IsSaved { get; private set; } = false;

    public string FirstName
    {
        get => _teacher.FirstName;
        set
        {
            _teacher.FirstName = value;
            OnPropertyChanged();
        }
    }

    public string LastName
    {
        get => _teacher.LastName;
        set
        {
            _teacher.LastName = value;
            OnPropertyChanged();
        }
    }

    public string Email
    {
        get => _teacher.Email;
        set
        {
            _teacher.Email = value;
            OnPropertyChanged();
        }
    }

    public string Phone
    {
        get => _teacher.Phone;
        set
        {
            _teacher.Phone = value;
            OnPropertyChanged();
        }
    }

    public bool IsCurator
    {
        get => _teacher.IsCurator;
        set
        {
            _teacher.IsCurator = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public TeacherDialogViewModel(Teacher teacher)
    {
        _teacher = teacher;
        _originalTeacher = new Teacher
        {
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            Email = teacher.Email,
            Phone = teacher.Phone,
            IsCurator = teacher.IsCurator
        };

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
    }

    private void Save()
    {
        IsSaved = true;
        CloseAction?.Invoke();
    }

    private void Cancel()
    {
        IsSaved = false;
        _teacher.FirstName = _originalTeacher.FirstName;
        _teacher.LastName = _originalTeacher.LastName;
        _teacher.Email = _originalTeacher.Email;
        _teacher.Phone = _originalTeacher.Phone;
        _teacher.IsCurator = _originalTeacher.IsCurator;

        CloseAction?.Invoke();
    }
}
