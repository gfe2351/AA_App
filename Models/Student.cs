namespace AA_App.Models;

/// <summary>Задания 1-8 и 17: публичный класс, наследник Person.</summary>
public class Student : Person
{
    private static int _counter;
    private string _group;

    /// <summary>Задание 8: только чтение после создания объекта.</summary>
    public int StudentId { get; }

    public string Group
    {
        get => _group;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Группа не может быть пустой");
            _group = value.Trim();
        }
    }

    public Student(string name, string surname, int age, string group) : base(name, surname, age)
    {
        _group = group;
        ValidateStudentData();          // задание 6
        _group = _group.Trim();
        StudentId = Interlocked.Increment(ref _counter);
    }

    // Задание 6: приватный метод проверки
    private void ValidateStudentData()
    {
        if (string.IsNullOrWhiteSpace(_name)) throw new ArgumentException("Имя обязательно");
        if (string.IsNullOrWhiteSpace(_surname)) throw new ArgumentException("Фамилия обязательна");
        if (_age < 15 || _age > 70) throw new ArgumentException("Возраст должен быть от 15 до 70 лет");
        if (string.IsNullOrWhiteSpace(_group)) throw new ArgumentException("Группа обязательна");
    }

    // Задание 7
    public string GetStudentInfo() => $"[ID {StudentId}] {DisplayBasicInfo()}, группа {_group}";

    public string FullNameViaProtectedInternal() => GetFullName();
    public int StatsViaPrivateProtected() => CalculateStatistics();

    internal static void ResetIdCounter() => _counter = 0;
}
