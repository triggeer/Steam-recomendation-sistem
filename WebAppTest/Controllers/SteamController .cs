using Microsoft.AspNetCore.Authorization;
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
		private readonly IGameService _gameService;
		private readonly IImportService _importService;
		private readonly IDataGainService _dataGainService;
		private readonly IUserService _userService;


		public SteamController(
		AppDbContext context,
		IImportService importService, 
		IDataGainService dataGainService,
		IUserService userService,
		IGameService gameService)
		{
			_context = context;
			_importService = importService;
			_dataGainService = dataGainService;
			_userService = userService;
			_gameService = gameService;
		}

		[Authorize(Roles = "Admin")]
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
			var game = await _gameService.GetGame(appId);
			if (game == null)
			{
				return View("NoData");
			}
			return View(game);
		}

		[Authorize(Roles = "Admin")]
		public IActionResult AddGame()
		{
			return View();
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> ImportGame(int appId)
		{
			await _gameService.ImportGameAsync(appId);
			//return View();
			return RedirectToAction("GameDetails", new {appId});
		}

		[Authorize(Roles = "Admin")]
		[HttpGet]
		public async Task<IActionResult> Get100ID()
		{
			List<int> ids = await _dataGainService.GetNewSpyGameIds();
			ViewBag.MyMessage = ids;
			await _importService.ImportNewGames(ids);
			return View();
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateData(int appId)
		{
			//await _gameService.UpdateGame(appId);
			//return RedirectToAction("GameDetails", new { appId });
			return View();
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> UpdateGame(int appId)
		{
			await _gameService.UpdateGamePrice(appId);
			await _context.SaveChangesAsync();
			return RedirectToAction("GameDetails", new { appId });
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateAllGames()
		{
			await _gameService.UpdateAllGamePrice();
			await _context.SaveChangesAsync();
			return RedirectToAction("Index", "Home");
		}


		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UserGameList()
		{
			return View();
		}


		[Authorize(Roles = "Admin")]
		[HttpGet]
		public async Task<IActionResult> FormGameTagsVector(int gameId)
		{
			var vector = await _gameService.GetGameTagsStrengh(gameId);
			if (vector == null)
			{
				return View("NoData");
			}
			return View(vector);
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateGamesImg()
		{
			await _gameService.UpdateAllImgAsync();
			return RedirectToPage("/Home/Index");
		}
	}
}