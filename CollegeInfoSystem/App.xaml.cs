using System.Configuration;
using System.Data;
using System.Windows;
using CollegeInfoSystem.Services;
using CollegeInfoSystem.ViewModels;
using CollegeInfoSystem.Views;
using Microsoft.EntityFrameworkCore;

namespace CollegeInfoSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Тут можна додати код ініціалізації програми.
            // Наприклад, обробку подій запуску, реєстрацію сервісів, тощо.
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var dbContext = new CollegeDbContext();
            var studentService = new StudentService(dbContext);
            var studentViewModel = new StudentViewModel(studentService);
            var studentsView = new StudentsView { DataContext = studentViewModel };

        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // Тут можна додати код, який виконується при завершенні програми.
            // Наприклад, збереження налаштувань, звільнення ресурсів.
        }

        // Тут можна додати інші методи та обробники подій.
    }
}
