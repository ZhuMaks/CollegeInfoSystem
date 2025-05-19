using CollegeInfoSystem.Models;
using CollegeInfoSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CollegeInfoSystem.ViewModels
{
    public class UsersViewModel : BaseViewModel, ILoadable
    {
        private readonly UserService _userService;
        private readonly CollegeDbContext _context;

        public event Action? OnClearFieldsRequested;

        public ObservableCollection<User> Users { get; } = new();

        private string _newUsername = "";
        public string NewUsername
        {
            get => _newUsername;
            set => SetProperty(ref _newUsername, value);
        }

        private string _newPassword = "";
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private string? _newRole;
        public string? NewRole
        {
            get => _newRole;
            set => SetProperty(ref _newRole, value);
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

        public IRelayCommand AddUserCommand { get; }
        public IRelayCommand DeleteUserCommand { get; }

        public UsersViewModel(UserService userService, CollegeDbContext context)
        {
            _userService = userService;
            _context = context;

            AddUserCommand = new RelayCommand(async () => await AddUser());
            DeleteUserCommand = new RelayCommand(async () => await DeleteUser(), () => SelectedUser != null);
        }

        public async Task LoadDataAsync()
        {
            Users.Clear();
            var allUsers = await _context.Users.ToListAsync();
            foreach (var user in allUsers)
                Users.Add(user);
        }

        private async Task AddUser()
        {
            if (!string.IsNullOrWhiteSpace(NewUsername) &&
                !string.IsNullOrWhiteSpace(NewPassword) &&
                !string.IsNullOrWhiteSpace(NewRole))
            {
                if (await _userService.RegisterAsync(NewUsername, NewPassword, NewRole))
                {
                    await LoadDataAsync();

                    NewUsername = "";
                    NewPassword = "";
                    NewRole = null;

                    OnClearFieldsRequested?.Invoke();
                }
            }
        }

        private async Task DeleteUser()
        {
            if (SelectedUser != null)
            {
                _context.Users.Remove(SelectedUser);
                await _context.SaveChangesAsync();
                await LoadDataAsync();
            }
        }
    }
}
