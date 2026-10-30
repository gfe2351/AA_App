using System.Security.Claims;
using AA_App.Data;
using AA_App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AA_App.Controllers;

[Authorize(Roles = "Admin")]
[AutoValidateAntiforgeryToken]
public class AdminController : Controller
{
    private static readonly string[] Roles = { "Admin", "Teacher", "Student" };
    private readonly CollegeDbContext _db;
    public AdminController(CollegeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.Runs = await _db.TaskRuns.CountAsync();
        return View(await _db.Users.OrderBy(u => u.Login).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> AddUser(string login, string password, string fullName, string role)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(fullName)) TempData["Err"] = "Логин и ФИО обязательны";
        else if (string.IsNullOrEmpty(password) || password.Length < 6) TempData["Err"] = "Пароль должен содержать минимум 6 символов";
        else if (!Roles.Contains(role)) TempData["Err"] = "Неизвестная роль";
        else if (await _db.Users.AnyAsync(u => u.Login == login.Trim())) TempData["Err"] = "Такой логин уже существует";
        else
        {
            var (hash, salt) = PasswordService.Hash(password);
            _db.Users.Add(new AppUser { Login = login.Trim(), FullName = fullName.Trim(), Role = role, PasswordHash = hash, Salt = salt });
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Пользователь создан";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        if (id.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            TempData["Err"] = "Нельзя удалить самого себя";
            return RedirectToAction(nameof(Index));
        }
        var u = await _db.Users.FindAsync(id);
        if (u != null) { _db.Users.Remove(u); await _db.SaveChangesAsync(); TempData["Ok"] = "Пользователь удалён"; }
        return RedirectToAction(nameof(Index));
    }
}
