using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Windows.Threading;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CollegeInfoSystem.ViewModels
{
    public class UsersViewModel : BaseViewModel, ILoadable
    {
        private readonly UserService _userService;
        private readonly CollegeDbContext _context;
        private DispatcherTimer _refreshTimer;

        public event Action? OnClearFieldsRequested;

        public ObservableCollection<User> Users { get; } = new();

        private string _newUsername = "";
        public string NewUsername
        {
            get => _newUsername;
            set
            {
                if (SetProperty(ref _newUsername, value))
                    AddUserCommand.NotifyCanExecuteChanged();
            }
        }

        private string _newPassword = "";
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (SetProperty(ref _newPassword, value))
                    AddUserCommand.NotifyCanExecuteChanged();
            }
        }

        private string? _newRole;
        public string? NewRole
        {
            get => _newRole;
            set
            {
                if (SetProperty(ref _newRole, value))
                    AddUserCommand.NotifyCanExecuteChanged();
            }
        }

        private User? _selectedUser;
        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                    DeleteUserCommand.NotifyCanExecuteChanged();
            }
        }

        private string _currentUserRole;
        public string CurrentUserRole
        {
            get => _currentUserRole;
            set
            {
                if (SetProperty(ref _currentUserRole, value))
                {
                    AddUserCommand.NotifyCanExecuteChanged();
                    DeleteUserCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public IRelayCommand AddUserCommand { get; }
        public IRelayCommand DeleteUserCommand { get; }

        public UsersViewModel(UserService userService, CollegeDbContext context, string currentUserRole)
        {
            _userService = userService;
            _context = context;
            _currentUserRole = currentUserRole;

            AddUserCommand = new RelayCommand(async () => await AddUser(), CanExecuteAddUser);
            DeleteUserCommand = new RelayCommand(async () => await DeleteUser(), CanExecuteDeleteUser);

            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(15);
            _refreshTimer.Tick += async (s, e) => await LoadDataAsync();
            _refreshTimer.Start();

        }

        public async Task LoadDataAsync()
        {
            Users.Clear();
            var allUsers = await _context.Users.ToListAsync();
            foreach (var user in allUsers)
                Users.Add(user);
        }

        private bool CanExecuteAddUser()
        {
            return CurrentUserRole == "admin"
                && !string.IsNullOrWhiteSpace(NewUsername)
                && !string.IsNullOrWhiteSpace(NewPassword)
                && !string.IsNullOrWhiteSpace(NewRole);
        }

        private bool CanExecuteDeleteUser()
        {
            return CurrentUserRole == "admin" && SelectedUser != null;
        }

        private async Task AddUser()
        {
            if (!CanExecuteAddUser()) return;

            if (await _userService.RegisterAsync(NewUsername, NewPassword, NewRole!))
            {
                await LoadDataAsync();

                NewUsername = "";
                NewPassword = "";
                NewRole = null;

                OnClearFieldsRequested?.Invoke();
            }
        }

        private async Task DeleteUser()
        {
            if (!CanExecuteDeleteUser()) return;

            _context.Users.Remove(SelectedUser!);
            await _context.SaveChangesAsync();
            await LoadDataAsync();
        }
    }
}
