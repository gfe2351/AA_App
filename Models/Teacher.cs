namespace AA_App.Models;

/// <summary>Задания 9, 10, 18.</summary>
public class Teacher : Person
{
    private string _subject;
    private int _experience;

    public string Subject => _subject;

    /// <summary>Задание 10: стаж меняется только через UpdateExperience().</summary>
    public int Experience => _experience;

    public Teacher(string name, string surname, int age, string subject, int experience)
        : base(name, surname, age)
    {
        if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("Предмет обязателен");
        if (experience < 0 || experience > 60) throw new ArgumentException("Стаж должен быть от 0 до 60 лет");
        _subject = subject.Trim();
        _experience = experience;
    }

    public void UpdateExperience(int years)
    {
        if (years < 0 || years > 60) throw new ArgumentException("Стаж должен быть от 0 до 60 лет");
        if (years < _experience) throw new ArgumentException("Стаж не может уменьшаться");
        _experience = years;
    }

    public string GetTeacherInfo() => $"Преподаватель {DisplayBasicInfo()}, предмет: {_subject}, стаж: {_experience} л.";
}
