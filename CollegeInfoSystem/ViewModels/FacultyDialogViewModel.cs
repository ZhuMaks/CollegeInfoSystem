using CollegeInfoSystem.Models;
using CollegeInfoSystem.ViewModels;
using CommunityToolkit.Mvvm.Input;

public class FacultyDialogViewModel : BaseViewModel
{
    private Faculty _faculty;
    private Faculty _originalFaculty;

    public Action CloseAction { get; set; }

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

    public void Save()
    {
        _faculty.FacultyName = FacultyName;
        CloseAction?.Invoke();
    }

    public void Cancel()
    {
        _faculty.FacultyName = _originalFaculty.FacultyName;
        CloseAction?.Invoke();
    }
}
