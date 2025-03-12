using CollegeInfoSystem.Views;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeInfoSystem.Services;

namespace CollegeInfoSystem.ViewModels;

public class MainViewModel : BaseViewModel
{
    private object? _currentView;
    public object? CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            OnPropertyChanged();
        }

    }


    public ICommand OpenStudentsViewCommand { get; }

    public MainViewModel()
    {
        OpenStudentsViewCommand = new RelayCommand(() => CurrentView = new StudentsView());
    }

}
