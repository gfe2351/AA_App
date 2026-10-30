namespace AA_App.Models;

/// <summary>Задания 14-15.</summary>
public class Subject
{
    private string _title;
    private int _hours;
    private Teacher _teacher;

    public Subject(string title, int hours)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Название предмета обязательно");
        if (hours <= 0 || hours > 1000) throw new ArgumentException("Количество часов должно быть от 1 до 1000");
        _title = title.Trim();
        _hours = hours;
    }

    public void AssignTeacher(Teacher teacher)
    {
        if (teacher == null) throw new ArgumentException("Преподаватель не может быть null");
        _teacher = teacher;
    }

    public string GetInfo()
        => $"Предмет «{_title}», {_hours} ч., преподаватель: {(_teacher == null ? "не назначен" : _teacher.Surname + " " + _teacher.Name)}";
}
