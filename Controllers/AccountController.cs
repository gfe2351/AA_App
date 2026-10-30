using System.Security.Claims;
using AA_App.Data;
using AA_App.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AA_App.Controllers;

[AutoValidateAntiforgeryToken]
public class AccountController : Controller
{
    private readonly CollegeDbContext _db;
    public AccountController(CollegeDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string login, string password)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Login == login);
        if (u == null || !PasswordService.Verify(password ?? "", u.PasswordHash, u.Salt))
        {
            ViewBag.Error = "Неверный логин или пароль";
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new Claim(ClaimTypes.Name, u.FullName),
            new Claim(ClaimTypes.Role, u.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return RedirectToAction("College", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("College", "Home");
    }
}
