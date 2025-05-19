using System.Windows;

namespace CollegeInfoSystem.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(string userRole)
        {
            InitializeComponent();

            if (userRole == "admin")
                UsersButton.Visibility = Visibility.Visible;
        }

    }
}
