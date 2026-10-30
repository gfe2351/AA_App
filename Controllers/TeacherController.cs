using AA_App.Data;
using AA_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AA_App.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class TeacherController : Controller
{
    private readonly CollegeDbContext _db;
    public TeacherController(CollegeDbContext db) => _db = db;

    public async Task<IActionResult> Index() => View(await _db.Teachers.OrderBy(t => t.Surname).ToListAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Add(string name, string surname, int age, string subject, int experience)
    {
        try
        {
            var t = new Teacher(name ?? "", surname ?? "", age, subject ?? "", experience);   // проверка доменным классом
            _db.Teachers.Add(new TeacherRecord { Name = t.Name, Surname = t.Surname, Age = t.Age, Subject = t.Subject, Experience = t.Experience });
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Преподаватель добавлен";
        }
        catch (ArgumentException ex) { TempData["Err"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await _db.Teachers.FindAsync(id);
        if (t != null) { _db.Teachers.Remove(t); await _db.SaveChangesAsync(); TempData["Ok"] = "Преподаватель удалён"; }
        return RedirectToAction(nameof(Index));
    }
}
