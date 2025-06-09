using CollegeInfoSystem.Models;
using CollegeInfoSystem.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;

public class FacultyDialogViewModel : BaseViewModel
{
    private Faculty _faculty;
    private Faculty _originalFaculty;

    public Action CloseAction { get; set; }
    public bool IsSaved { get; private set; } = false;

    public Faculty Faculty
    {
        get => _faculty;
        set
        {
            _faculty = value;
            OnPropertyChanged();
        }
    }

    public string FacultyName
    {
        get => _faculty.FacultyName;
        set
        {
            _faculty.FacultyName = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public FacultyDialogViewModel(Faculty faculty)
    {
        _faculty = faculty;
        _originalFaculty = new Faculty
        {
            FacultyName = faculty.FacultyName
        };

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
    }

    private bool ValidateFields()
    {
        return !string.IsNullOrWhiteSpace(FacultyName);
    }

    private void Save()
    {
        try
        {
            if (!ValidateFields())
            {
                MessageBox.Show("Усі поля мають бути заповнені!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsSaved = true;
            CloseAction?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Сталася помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel()
    {
        IsSaved = false;
        FacultyName = _originalFaculty.FacultyName;
        CloseAction?.Invoke();
    }
}
