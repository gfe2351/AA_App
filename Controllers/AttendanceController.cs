using AA_App.Data;
using AA_App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AA_App.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class AttendanceController : Controller
{
    private readonly CollegeDbContext _db;
    public AttendanceController(CollegeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var students = await _db.Students.Include(s => s.Group).Include(s => s.Attendance)
                                .OrderBy(s => s.Surname).ToListAsync();
        var mgr = new AttendanceManager();
        foreach (var s in students)
            foreach (var a in s.Attendance)
                mgr.Add(s.Id.ToString(), a.Date, a.Present);

        ViewBag.Stats = mgr.GetStatistics().ToDictionary(x => x.Student);
        return View(students);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Mark(int studentId, string date, bool present)
    {
        try
        {
            var s = await _db.Students.Include(x => x.Attendance).FirstOrDefaultAsync(x => x.Id == studentId)
                    ?? throw new ArgumentException("Студент не найден");
            var key = s.Id.ToString();

            var mgr = new AttendanceManager();
            foreach (var a in s.Attendance) mgr.Add(key, a.Date, a.Present);

            var existing = s.Attendance.FirstOrDefault(a => a.Date == date);
            if (existing != null)
            {
                mgr.Change(key, date, present);
                existing.Present = present;
            }
            else
            {
                mgr.Add(key, date, present);
                _db.Attendance.Add(new AttendanceRecord { StudentId = s.Id, Date = date, Present = present });
            }
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Отметка сохранена";
        }
        catch (Exception ex) when (ex is ArgumentException || ex is FormatException || ex is InvalidOperationException)
        {
            TempData["Err"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
