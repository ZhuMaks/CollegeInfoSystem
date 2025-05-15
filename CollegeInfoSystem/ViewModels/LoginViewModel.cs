using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CollegeInfoSystem.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly UserService _userService;

        [ObservableProperty]
        private string username = string.Empty;

        public string Password { private get; set; } = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public event Action<string>? LoginSucceeded;

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel()
        {
            _userService = new UserService(new CollegeDbContext());
            LoginCommand = new RelayCommand(async () => await LoginAsync());
            RegisterCommand = new RelayCommand(async () => await RegisterAsync());
        }

        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter username and password.";
                return;
            }

            var user = await _userService.AuthenticateAsync(Username, Password);

            if (user != null)
            {
                LoginSucceeded?.Invoke(user.Role);
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
            }
        }

        private async Task RegisterAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter username and password.";
                return;
            }

            var success = await _userService.RegisterAsync(Username, Password, "guest");

            if (success)
            {
                LoginSucceeded?.Invoke("guest");
            }
            else
            {
                ErrorMessage = "User already exists.";
            }
        }
    }
}
