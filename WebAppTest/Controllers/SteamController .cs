using Microsoft.AspNetCore.Mvc;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Services;
using WebAppTest.Models;

namespace WebAppTest.Controllers
{
	public class SteamController : Controller
	{
		private readonly ISteamService _steamService;
		private readonly IImportService _importService;
		private readonly IDataGainService _dataGainService;
		private readonly IUserService _userService;

		public SteamController(
		ISteamService steamService, 
		IImportService importService, 
		IDataGainService dataGainService,
		IUserService userService)
		{
			_steamService = steamService;
			_importService = importService;
			_dataGainService = dataGainService;
			_userService = userService;
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
			await _steamService.UpdateGame(appId);
			return RedirectToAction("GameDetails", new { appId });

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

		public async Task<IActionResult> GetUserGameTagsValue(string userId)
		{
			var tags = await _userService.GetUserGameTags(userId);
			return View(tags);
		}
	}
}