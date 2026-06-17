using Microsoft.AspNetCore.Mvc;
using WebAppTest.Interfaces;
using WebAppTest.Services;

namespace WebAppTest.Controllers
{
	public class RecommendationController : Controller
	{
		private readonly IUserService _userService;
		private readonly IRecommendationService _recommendationService;

		public RecommendationController(
		IUserService userService,
		IRecommendationService recommendationService)
		{
			_userService = userService;
			_recommendationService = recommendationService;
		}


		public IActionResult Index()
		{
			return View();
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
	}
}
