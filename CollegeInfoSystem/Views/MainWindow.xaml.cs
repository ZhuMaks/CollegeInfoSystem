using System.Windows;
using CollegeInfoSystem.ViewModels;

namespace CollegeInfoSystem.Views
{
    public partial class MainWindow : Window
    {
        private readonly ResourceDictionary lightTheme = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Themes/LightTheme.xaml")
        };

        private readonly ResourceDictionary darkTheme = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Themes/DarkTheme.xaml")
        };

        public MainWindow(string userRole)
        {
            InitializeComponent();

            ApplyTheme(lightTheme);

            if (userRole == "admin")
                UsersButton.Visibility = Visibility.Visible;
        }

        private void ApplyTheme(ResourceDictionary theme)
        {
            var existing = Resources.MergedDictionaries.FirstOrDefault(d =>
                d.Source != null && d.Source.OriginalString.Contains("Theme"));

            if (existing != null)
                Resources.MergedDictionaries.Remove(existing);

            Resources.MergedDictionaries.Add(theme);
        }

        private void ThemeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ApplyTheme(darkTheme);
        }

        private void ThemeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplyTheme(lightTheme);
        }
    }
}
