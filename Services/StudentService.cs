using AA_App.Data;
using AA_App.Models;

namespace AA_App.Services;

public class StudentService
{
    private readonly CollegeDbContext _db;
    public StudentService(CollegeDbContext db) => _db = db;

    /// <summary>Проверяет данные доменным классом Student и сохраняет в SQLite.</summary>
    public async Task AddAsync(string name, string surname, int age, int groupId)
    {
        var group = await _db.Groups.FindAsync(groupId) ?? throw new ArgumentException("Группа не найдена");
        var checkedStudent = new Student(name ?? "", surname ?? "", age, group.Title);
        _db.Students.Add(new StudentRecord
        {
            Name = checkedStudent.Name,
            Surname = checkedStudent.Surname,
            Age = checkedStudent.Age,
            GroupId = group.Id
        });
        await _db.SaveChangesAsync();
    }
}
