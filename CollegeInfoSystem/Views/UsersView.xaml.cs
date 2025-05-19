using System.Windows;
using System.Windows.Controls;
using CollegeInfoSystem.ViewModels;

namespace CollegeInfoSystem.Views
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                if (DataContext is UsersViewModel vm)
                {
                    vm.OnClearFieldsRequested += () =>
                    {
                        passwordBox.Password = "";
                        roleComboBox.SelectedIndex = -1;
                        usernameTextBox.Text = "";
                    };
                }
            };
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsersViewModel vm)
            {
                vm.NewPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}
