using CollegeInfoSystem.Models;
using CollegeInfoSystem.ViewModels;
using CommunityToolkit.Mvvm.Input;

public class FacultyDialogViewModel : BaseViewModel
{
    private Faculty _faculty;
    private Faculty _originalFaculty;

    public Action CloseAction { get; set; }
    public bool IsSaved { get; private set; } = false; // Флаг для перевірки

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

    private void Save()
    {
        IsSaved = true; // Позначаємо, що дані збережені
        CloseAction?.Invoke();
    }

    private void Cancel()
    {
        IsSaved = false; // Позначаємо, що відмінили
        CloseAction?.Invoke();
    }
}
