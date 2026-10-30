namespace AA_App.Models;

/// <summary>Задание 28: журнал оценок, список закрыт, допустимы оценки 2-5.</summary>
public class GradeBook
{
    private readonly List<int> _grades = new();

    public GradeBook() { }

    public GradeBook(IEnumerable<int> initial)
    {
        foreach (var g in initial) AddGrade(g);
    }

    public IReadOnlyList<int> Grades => _grades.AsReadOnly();
    public double Average => _grades.Count == 0 ? 0 : _grades.Average();

    public void AddGrade(int grade)
    {
        if (grade < 2 || grade > 5) throw new ArgumentException("Допустимы оценки от 2 до 5");
        _grades.Add(grade);
    }

    public bool RemoveGrade(int grade)
    {
        if (grade < 2 || grade > 5) throw new ArgumentException("Допустимы оценки от 2 до 5");
        return _grades.Remove(grade);
    }
}
