using AA_App.Services;

namespace AA_App.Data;

public static class DbSeeder
{
    public static void Seed(CollegeDbContext db)
    {
        if (db.Users.Any()) return;

        AppUser U(string login, string pass, string name, string role)
        {
            var (hash, salt) = PasswordService.Hash(pass);
            return new AppUser { Login = login, FullName = name, Role = role, PasswordHash = hash, Salt = salt };
        }

        db.Users.AddRange(
            U("admin", "admin123", "Администратор системы", "Admin"),
            U("teacher", "teacher123", "Преподаватель Демо", "Teacher"),
            U("student", "student123", "Студент Демо", "Student"));

        var g1 = new GroupRecord { Title = "ИС-21", Course = 2 };
        var g2 = new GroupRecord { Title = "ПО-11", Course = 1 };
        db.Groups.AddRange(g1, g2);

        var s1 = new StudentRecord { Name = "Иван", Surname = "Петров", Age = 19, Group = g1 };
        var s2 = new StudentRecord { Name = "Анна", Surname = "Сидорова", Age = 18, Group = g1 };
        var s3 = new StudentRecord { Name = "Пётр", Surname = "Орлов", Age = 17, Group = g2 };
        db.Students.AddRange(s1, s2, s3);

        db.Teachers.Add(new TeacherRecord { Name = "Мария", Surname = "Иванова", Age = 42, Subject = "Программирование", Experience = 15 });

        db.Grades.AddRange(
            new GradeRecord { Student = s1, Subject = "Программирование", Value = 5, Date = DateTime.Today },
            new GradeRecord { Student = s1, Subject = "Математика", Value = 4, Date = DateTime.Today },
            new GradeRecord { Student = s2, Subject = "Программирование", Value = 4, Date = DateTime.Today });

        var d = DateTime.Today.ToString("yyyy-MM-dd");
        db.Attendance.AddRange(
            new AttendanceRecord { Student = s1, Date = d, Present = true },
            new AttendanceRecord { Student = s2, Date = d, Present = false });

        db.SaveChanges();
    }
}
