using AA_App.Data;
using AA_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AA_App.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class GradeController : Controller
{
    private readonly CollegeDbContext _db;
    public GradeController(CollegeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var students = await _db.Students.Include(s => s.Group).Include(s => s.Grades)
                                .OrderBy(s => s.Surname).ToListAsync();
        return View(students);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Add(int studentId, string subject, int value)
    {
        try
        {
            var s = await _db.Students.Include(x => x.Grades).FirstOrDefaultAsync(x => x.Id == studentId)
                    ?? throw new ArgumentException("Студент не найден");
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("Предмет обязателен");

            var book = new GradeBook(s.Grades.Select(g => g.Value));
            book.AddGrade(value);                                    // проверка 2..5 в GradeBook

            _db.Grades.Add(new GradeRecord { StudentId = s.Id, Subject = subject.Trim(), Value = value, Date = DateTime.Today });
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Оценка добавлена";
        }
        catch (ArgumentException ex) { TempData["Err"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        var g = await _db.Grades.FindAsync(id);
        if (g != null) { _db.Grades.Remove(g); await _db.SaveChangesAsync(); TempData["Ok"] = "Оценка удалена"; }
        return RedirectToAction(nameof(Index));
    }
}
