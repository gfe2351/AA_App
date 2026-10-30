using AA_App.Data;
using AA_App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AA_App.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class StudentController : Controller
{
    private readonly CollegeDbContext _db;
    private readonly StudentService _service;
    public StudentController(CollegeDbContext db, StudentService service) { _db = db; _service = service; }

    public async Task<IActionResult> Index()
    {
        ViewBag.Groups = await _db.Groups.OrderBy(g => g.Title).ToListAsync();
        var list = await _db.Students.Include(s => s.Group).OrderBy(s => s.Surname).ToListAsync();
        return View(list);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Add(string name, string surname, int age, int groupId)
    {
        try
        {
            await _service.AddAsync(name, surname, age, groupId);
            TempData["Ok"] = "Студент добавлен";
        }
        catch (ArgumentException ex) { TempData["Err"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Students.FindAsync(id);
        if (s != null) { _db.Students.Remove(s); await _db.SaveChangesAsync(); TempData["Ok"] = "Студент удалён"; }
        return RedirectToAction(nameof(Index));
    }
}
