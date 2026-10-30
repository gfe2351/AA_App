namespace AA_App.Models;

/// <summary>Задания 21-22: internal-класс, настройки меняются только внутри сборки.</summary>
internal class CollegeSettings
{
    public string CollegeName { get; private set; }
    public string AcademicYear { get; private set; }

    internal CollegeSettings(string collegeName, string academicYear)
    {
        Update(collegeName, academicYear);
    }

    internal void Update(string collegeName, string academicYear)
    {
        if (string.IsNullOrWhiteSpace(collegeName)) throw new ArgumentException("Название колледжа обязательно");
        if (string.IsNullOrWhiteSpace(academicYear)) throw new ArgumentException("Учебный год обязателен");
        CollegeName = collegeName.Trim();
        AcademicYear = academicYear.Trim();
    }

    internal string Describe() => $"{CollegeName}, {AcademicYear} уч. год";
}
