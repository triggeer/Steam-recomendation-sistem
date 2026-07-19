using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Services;


namespace WebAppTest.Controllers
{
	public class RecommendationController : Controller
	{
		private readonly AppDbContext _context;
		private readonly IUserService _userService;
		private readonly IRecommendationService _recommendationService;

		public RecommendationController(
		AppDbContext context,
		IUserService userService,
		IRecommendationService recommendationService)
		{
			_context = context;
			_userService = userService;
			_recommendationService = recommendationService;
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
		public async Task<IActionResult> FormRecomendations(string userId, int gameId)
		{
			var userVector = await _recommendationService.FormUserTagVector(userId);
			var gameVector = await _userService.GetGameTagsStrengh(gameId);
			Dictionary<string, double> userTags = userVector.TagStrength;
			Dictionary<string, double> gameTags = gameVector[gameId.ToString()];
			double result = await _recommendationService.CosSimilarity(userTags, gameTags);
			return View(result);
		}

		[HttpGet]
		public async Task<IActionResult> FormRecommendationList(string userId)
		{
			//var cosSimList = await _recommendationService.FormRecommendationListAsync(userId);
			var cosSimList = await _recommendationService.ForUniqueRecomendationsAsync(userId);
			return View(cosSimList);
		}

		[HttpGet]
		public async Task<IActionResult> FormRecomendationsOnTag(string userId, int tagId)
		{
			RecommendationsOnTeg list = await _recommendationService.FormRecomendationsOnTagAsync(userId, tagId);	
			return View(list);
		}
	}
}
