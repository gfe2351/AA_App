using AA_App.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AA_App.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class GroupController : Controller
{
    private readonly CollegeDbContext _db;
    public GroupController(CollegeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
        => View(await _db.Groups.Include(g => g.Students).OrderBy(g => g.Title).ToListAsync());

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Add(string title, int course)
    {
        try
        {
            var g = new AA_App.Models.Group(title ?? "", course);     // проверка доменным классом
            _db.Groups.Add(new GroupRecord { Title = g.Title, Course = g.Course });
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Группа добавлена";
        }
        catch (ArgumentException ex) { TempData["Err"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var g = await _db.Groups.FindAsync(id);
        if (g != null) { _db.Groups.Remove(g); await _db.SaveChangesAsync(); TempData["Ok"] = "Группа удалена"; }
        return RedirectToAction(nameof(Index));
    }
}
