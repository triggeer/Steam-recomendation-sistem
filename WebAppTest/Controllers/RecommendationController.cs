using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Models;
using WebAppTest.Services;


namespace WebAppTest.Controllers
{
	public class RecommendationController : Controller
	{
		private readonly AppDbContext _context;
		private readonly IUserService _userService;
		private readonly IRecommendationService _recommendationService;
		private readonly IDataGainService _dataGainService;
		private readonly IUserListService _userListService;

		public RecommendationController(
		AppDbContext context,
		IUserService userService,
		IRecommendationService recommendationService,
		IDataGainService dataGainService,
		IUserListService userListService)
		{
			_context = context;
			_userService = userService;
			_recommendationService = recommendationService;
			_dataGainService = dataGainService;
			_userListService = userListService;
		}


		public async Task<IActionResult> Index()
		{
			// 1. Создаем пустой объект модели
			var model = new TagCheckBox();

			// 2. Заполняем список тегов напрямую из базы
			var tags = await _context.Tags.ToListAsync();
			model.TagFromDb = new SelectList(tags, "Id", "Name");

			// 3. Передаем заполненную модель в View
			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> FormRecommendationList(string userLink)
		{
			string userId = await _dataGainService.TransformLinkToId(userLink);
			var cosSimList = await _recommendationService.ForUniqueRecomendationsAsync(userId);
			return View(cosSimList);
		}

		[HttpGet]
		public async Task<IActionResult> FormRecomendationsOnTag(string userId, int tagId)
		{
			RecommendationsOnTeg list = await _recommendationService.FormRecomendationsOnTagAsync(userId, tagId);	
			return View(list);
		}


		[HttpGet]
		public async Task UpdateGameVectors()
		{
			await _recommendationService.AddGameVector();
		}


		public async Task<IActionResult> ShowRecomendationsInARow(string userLink)
		{
			string userId = await _dataGainService.TransformLinkToId(userLink);

			List<RecommendationDto> recommendations = await _recommendationService.ForUniqueRecomendationsAsync(userId);

			HttpContext.Session.SetString("Recommendations", JsonSerializer.Serialize(recommendations));

			return RedirectToAction("RecomendationsInARow", new { index = 0 });
		}

		

		[HttpGet("Recomendation/RecomendationsInARow/{index}")]
		public async Task<IActionResult> RecomendationsInARow(int index)
		{
			string json = HttpContext.Session.GetString("Recommendations");

			List<RecommendationDto> recommendations = JsonSerializer.Deserialize<List<RecommendationDto>>(json);

			if (index >= recommendations.Count)
				return RedirectToAction("Index");

			RecommendationDto recommendation= recommendations[index];

			Game? game = await _context.Games
				.AsNoTracking()
				.Include(gt => gt.GameTags)
					.ThenInclude(t  => t.Tag)
				.FirstOrDefaultAsync(x => x.Id == recommendation.GameId);

			if (game == null)
				return NotFound();

			var model = new GameViewModel
			{
				Game = game,
				Index = index,
				ListSize = recommendations.Count(),
				ListId = null
			};

			return View("Game", model);
		}


		[HttpGet]
		public async Task<IActionResult> ShowPreviousLists(string userLink)
		{
			string userId = await _dataGainService.TransformLinkToId(userLink);

			List<PrevListDto> prevLists = await _userListService.CollectFormedList(userId);

			HttpContext.Session.SetString("Recommendations", JsonSerializer.Serialize(prevLists));

			return View(prevLists);

		}

		[HttpGet]
		public async Task<IActionResult> ShowChosenList(int listId, int index = 0)
		{
			string json = HttpContext.Session.GetString("Recommendations");

			List<PrevListDto> lists = JsonSerializer.Deserialize<List<PrevListDto>>(json);

			var chosenList = lists.FirstOrDefault(l => l.Id == listId);

			var chosenId = chosenList.Games[index].Id;

			var game = await _context.Games
				.Include(gt => gt.GameTags)
						.ThenInclude(t => t.Tag)
				.FirstOrDefaultAsync(x => x.Id == chosenId);

			var model = new GameViewModel
			{
				Game = game,
				Index = index,
				ListSize = chosenList.Games.Count,
				ListId = listId
			};

			return View("Game", model);
		}

	}
}
