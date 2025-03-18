using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CollegeInfoSystem.ViewModels;

public interface ILoadable
{
    Task LoadDataAsync();
}
public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged = delegate { };

    protected void OnPropertyChanged(string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}




