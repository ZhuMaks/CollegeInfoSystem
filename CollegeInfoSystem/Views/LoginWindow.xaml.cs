using System.Windows;
using System.Windows.Controls;
using CollegeInfoSystem.ViewModels;

namespace CollegeInfoSystem.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            ApplyTheme(App.CurrentTheme);
            if (DataContext is LoginViewModel vm)
            {
                vm.LoginSucceeded += OnLoginSucceeded;
            }
        }

        private void OnLoginSucceeded(string userRole)
        {
            App.CurrentUserRole = userRole;
            DialogResult = true;
            Close();        
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm && sender is PasswordBox passwordBox)
            {
                vm.Password = passwordBox.Password;
            }
        }
        private void ApplyTheme(string themeName)
        {
            var dict = new ResourceDictionary();

            if (themeName == "dark")
                dict.Source = new Uri("/CollegeInfoSystem;component/Themes/DarkTheme.xaml", UriKind.RelativeOrAbsolute);
            else
                dict.Source = new Uri("/CollegeInfoSystem;component/Themes/LightTheme.xaml", UriKind.RelativeOrAbsolute);

            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(dict);
        }

    }
}
