using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Models;
using WebAppTest.Services;

namespace WebAppTest.Controllers
{
	public class SteamController : Controller
	{
		private readonly AppDbContext _context;
		private readonly ISteamService _steamService;
		private readonly IImportService _importService;
		private readonly IDataGainService _dataGainService;
		private readonly IUserService _userService;
		private readonly IDBService _dbService;


		public SteamController(
		AppDbContext context,
		ISteamService steamService, 
		IImportService importService, 
		IDataGainService dataGainService,
		IUserService userService,
		IDBService dbService)
		{
			_context = context;
			_steamService = steamService;
			_importService = importService;
			_dataGainService = dataGainService;
			_userService = userService;
			_dbService = dbService;
		}

		[HttpGet]
		public IActionResult FindGame()
		{
			return View();
		}

		[HttpGet]
		// IActionResult - что вернуть пользователю
		public async Task<IActionResult> GameDetails(int appId)

		{
			// ждем ответа от GetGame и возвращаем вид (открываем cshtml) и передаем туда model = game
			var game = await _steamService.GetGame(appId);
			if (game == null)
			{
				return View("NoData");
			}
			return View(game);
		}

		
		public IActionResult AddGame()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ImportGame(int appId)
		{
			await _steamService.ImportGameAsync(appId);
			//return View();
			return RedirectToAction("GameDetails", new {appId});
		}

		[HttpGet]
		public async Task<IActionResult> Get100ID()
		{
			List<int> ids = await _importService.Get100Games();
			ViewBag.MyMessage = ids;
			await _importService.Import100Games(ids);
			//return RedirectToAction("Index");
			return View();
		}

		public async Task<IActionResult> UpdateData(int appId)
		{
			//await _steamService.UpdateGame(appId);
			//return RedirectToAction("GameDetails", new { appId });
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> UpdateGame(int appId)
		{
			await _dbService.UpdateGamePrice(appId);
			await _context.SaveChangesAsync();
			return RedirectToAction("GameDetails", new { appId });
		}

		public async Task<IActionResult> UpdateAllGames()
		{
			await _dbService.UpdateAllGamePrice();
			await _context.SaveChangesAsync();
			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		public async Task<IActionResult> GetUserGames(string userId)
		{
			var games = await _userService.GetUserGames(userId);
			return View(games);
		}

		public async Task<IActionResult> UserGameList()
		{
			return View();
		}


		[HttpGet]
		public async Task<IActionResult> FormGameTagsVector(int gameId)
		{
			var vector = await _userService.GetGameTagsStrengh(gameId);
			if (vector == null)
			{
				return View("NoData");
			}
			return View(vector);
		}

		public async Task<IActionResult> UpdateGamesImg()
		{
			await _dbService.UpdateAllImgAsync();
			return RedirectToPage("/Home/Index");
		}
	}
}