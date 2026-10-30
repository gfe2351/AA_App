using AA_App.Data;
using AA_App.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AA_App.Controllers;

public class HomeController : Controller
{
    private readonly CollegeDbContext _db;
    public HomeController(CollegeDbContext db) => _db = db;

    // Список всех 30 заданий
    public IActionResult Index() => View(TaskCatalog.All);

    // Главная страница колледжа (задание 30)
    public async Task<IActionResult> College()
    {
        ViewBag.Students = await _db.Students.CountAsync();
        ViewBag.Teachers = await _db.Teachers.CountAsync();
        ViewBag.Groups = await _db.Groups.CountAsync();
        ViewBag.Grades = await _db.Grades.CountAsync();
        return View();
    }

    public async Task<IActionResult> History()
    {
        var runs = await _db.TaskRuns.OrderByDescending(r => r.Id).Take(100).ToListAsync();
        return View(runs);
    }
}
