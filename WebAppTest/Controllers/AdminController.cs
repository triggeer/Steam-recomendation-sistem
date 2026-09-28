using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class AdminController : Controller
{
	private readonly IConfiguration _configuration;

	public AdminController(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	[HttpGet]
	public IActionResult Login()
	{
		return View();
	}

	[Authorize(Roles = "Admin")]
	[HttpPost]
	public async Task<IActionResult> Logout()
	{
		await HttpContext.SignOutAsync(
			CookieAuthenticationDefaults.AuthenticationScheme);

		return RedirectToAction("Login");
	}

	[HttpPost]
	public async Task<IActionResult> Login(string login, string password)
	{
		string? adminLogin = _configuration["Admin:Login"];
		string? adminPassword = _configuration["Admin:Password"];

		if (login != adminLogin || password != adminPassword)
		{
			ModelState.AddModelError("", "Неверный логин или пароль");
			return View();
		}

		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.Name, login),
			new Claim(ClaimTypes.Role, "Admin")
		};

		var identity = new ClaimsIdentity(
			claims,
			CookieAuthenticationDefaults.AuthenticationScheme);

		var principal = new ClaimsPrincipal(identity);

		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			principal);

		return RedirectToAction("Index");
	}

	[Authorize(Roles = "Admin")]
	public IActionResult Index()
	{
		return View();
	}
}