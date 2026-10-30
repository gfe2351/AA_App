namespace AA_App.Models;

/// <summary>Задания 11-13: список студентов закрыт.</summary>
public class Group
{
    private readonly List<Student> _students = new();

    public string Title { get; }
    public int Course { get; }

    public IReadOnlyList<Student> Students => _students.AsReadOnly();

    public Group(string title, int course)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Название группы обязательно");
        if (course < 1 || course > 6) throw new ArgumentException("Курс должен быть от 1 до 6");
        Title = title.Trim();
        Course = course;
    }

    public bool AddStudent(Student student)
    {
        if (student == null) throw new ArgumentException("Студент не может быть null");
        if (_students.Any(s => Same(s.Name, s.Surname, student.Name, student.Surname))) return false;
        _students.Add(student);
        return true;
    }

    public bool RemoveStudent(string name, string surname)
        => _students.RemoveAll(s => Same(s.Name, s.Surname, name, surname)) > 0;

    private static bool Same(string n1, string s1, string n2, string s2)
        => string.Equals(n1?.Trim(), n2?.Trim(), StringComparison.OrdinalIgnoreCase)
        && string.Equals(s1?.Trim(), s2?.Trim(), StringComparison.OrdinalIgnoreCase);
}
