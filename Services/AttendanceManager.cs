using System.Globalization;

namespace AA_App.Services;

public record AttendanceStat(string Student, int Total, int Present)
{
    public double Percent => Total == 0 ? 0 : Math.Round(100.0 * Present / Total, 1);
}

/// <summary>Задание 29: данные о посещаемости приватны, доступ только через методы.</summary>
public class AttendanceManager
{
    private readonly Dictionary<(string Student, DateOnly Date), bool> _records = new();

    public void Add(string student, string date, bool present)
    {
        var key = Key(student, date);
        if (_records.ContainsKey(key))
            throw new InvalidOperationException($"Отметка для «{key.Item1}» на {date} уже есть — используйте изменение");
        _records[key] = present;
    }

    public void Change(string student, string date, bool present)
    {
        var key = Key(student, date);
        if (!_records.ContainsKey(key))
            throw new InvalidOperationException($"Отметка для «{key.Item1}» на {date} не найдена — сначала добавьте её");
        _records[key] = present;
    }

    public IReadOnlyList<AttendanceStat> GetStatistics()
        => _records.GroupBy(r => r.Key.Student)
                   .Select(g => new AttendanceStat(g.Key, g.Count(), g.Count(x => x.Value)))
                   .OrderBy(s => s.Student)
                   .ToList();

    private static (string, DateOnly) Key(string student, string date)
    {
        if (string.IsNullOrWhiteSpace(student)) throw new ArgumentException("Студент обязателен");
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            throw new FormatException("Дата должна быть в формате гггг-мм-дд");
        return (student.Trim(), d);
    }
}
