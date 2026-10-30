#pragma warning disable CS0414
namespace AA_App.Models;

/// <summary>Базовый класс (задания 16, 19, 20, 23, 25).</summary>
public class Person
{
    protected string _name = "";                       // protected: доступно наследникам
    protected string _surname = "";
    protected int _age;
    private string _passportNote = "скрыто";           // private: только внутри Person
    internal string InternalTag => "internal-член Person"; // internal: внутри сборки

    public Person(string name, string surname, int age)
    {
        Name = name;
        Surname = surname;
        SetAge(age);
    }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Имя не может быть пустым");
            _name = value.Trim();
        }
    }

    public string Surname
    {
        get => _surname;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Фамилия не может быть пустой");
            _surname = value.Trim();
        }
    }

    public int Age
    {
        get => _age;
        set => SetAge(value);
    }

    /// <summary>Задание 4: возраст только от 15 до 70.</summary>
    public void SetAge(int age)
    {
        if (age < 15 || age > 70) throw new ArgumentException("Возраст должен быть от 15 до 70 лет");
        _age = age;
    }

    public string Describe() => DisplayBasicInfo();

    protected string DisplayBasicInfo() => $"{_surname} {_name}, {_age} лет";   // задание 19

    protected internal string GetFullName() => $"{_surname} {_name}";           // задание 23

    private protected int CalculateStatistics() => _age * 365;                  // задание 25 (возраст в днях)
}
