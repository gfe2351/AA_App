namespace AA_App.Data;

public class GroupRecord
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Course { get; set; }
    public List<StudentRecord> Students { get; set; } = new();
}

public class StudentRecord
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public int Age { get; set; }
    public int GroupId { get; set; }
    public GroupRecord Group { get; set; }
    public List<GradeRecord> Grades { get; set; } = new();
    public List<AttendanceRecord> Attendance { get; set; } = new();
}

public class TeacherRecord
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public int Age { get; set; }
    public string Subject { get; set; }
    public int Experience { get; set; }
}

public class GradeRecord
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public StudentRecord Student { get; set; }
    public string Subject { get; set; }
    public int Value { get; set; }
    public DateTime Date { get; set; }
}

public class AttendanceRecord
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public StudentRecord Student { get; set; }
    public string Date { get; set; }      // гггг-мм-дд
    public bool Present { get; set; }
}

public class AppUser
{
    public int Id { get; set; }
    public string Login { get; set; }
    public string FullName { get; set; }
    public string Role { get; set; }      // Admin | Teacher | Student
    public string PasswordHash { get; set; }
    public string Salt { get; set; }
}

public class TaskRun
{
    public int Id { get; set; }
    public int TaskNumber { get; set; }
    public string TaskTitle { get; set; }
    public int RowsCount { get; set; }
    public string Summary { get; set; }
    public DateTime CreatedAt { get; set; }
}
