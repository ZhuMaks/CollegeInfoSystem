using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CollegeInfoSystem.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Інформаційна система коледжу";
}
