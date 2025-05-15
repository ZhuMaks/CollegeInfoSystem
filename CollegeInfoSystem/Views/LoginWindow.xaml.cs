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
    }
}
