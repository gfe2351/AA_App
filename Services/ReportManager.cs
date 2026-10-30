namespace AA_App.Services;

/// <summary>Задание 24: protected internal метод формирования отчёта.</summary>
public class ReportManager
{
    protected internal string GenerateReport(string title, IEnumerable<string> lines)
    {
        var list = lines.ToList();
        return $"=== {title} ===\n"
             + string.Join("\n", list.Select((l, i) => $"{i + 1}. {l}"))
             + $"\nВсего записей: {list.Count}";
    }
}

/// <summary>Наследник, которому доступен GenerateReport().</summary>
public class GradeReport : ReportManager
{
    public string Build(string title, IEnumerable<string> lines) => GenerateReport(title, lines);
}
