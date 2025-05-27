# College Information System

**College Information System** — це десктопний додаток для управління інформацією коледжу. Проєкт реалізований на C# з використанням WPF та Entity Framework Core з базою даних SQL Server.

---

## Опис

Додаток дозволяє управляти студентами, викладачами, групами, розкладом, факультетами та працівниками коледжу. Є підтримка ролей користувачів (admin, teacher, guest) з різними рівнями доступу.

### Основні функції:
- CRUD операції для студентів, викладачів, груп, розкладу, факультетів і працівників
- Пошук та фільтрація даних
- Генерація звітів у форматах Excel
- Імпорт даних із Excel
- Підтримка тем (світла, темна)
- Авторизація та розмежування прав доступу
- Оновлення даних у реальному часі

---

## Технології

- .NET 8
- C#
- WPF (Windows Presentation Foundation)
- Entity Framework Core
- SQL Server (локально або Azure SQL)
- CommunityToolkit.Mvvm (MVVM framework)
- ClosedXML (для роботи з Excel)

---

## Встановлення

1. Клонуйте репозиторій:

```bash
git clone https://github.com/your-username/CollegeInfoSystem.git
cd CollegeInfoSystem
