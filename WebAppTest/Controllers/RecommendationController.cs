using Microsoft.AspNetCore.Mvc;
using WebAppTest.Interfaces;
using WebAppTest.Services;

namespace WebAppTest.Controllers
{
	public class RecommendationController : Controller
	{
		private readonly ISteamService _steamService;
		private readonly IImportService _importService;
		private readonly IDataGainService _dataGainService;
		private readonly IUserService _userService;
		private readonly IRecommendationService _recommendationService;

		public RecommendationController(
		ISteamService steamService,
		IImportService importService,
		IDataGainService dataGainService,
		IUserService userService,
		IRecommendationService recommendationService)
		{
			_steamService = steamService;
			_importService = importService;
			_dataGainService = dataGainService;
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
			var userVector = await _userService.FormUserTagVector(userId);
			var gameVector = await _userService.GetGameTagsStrengh(gameId);
			var result = await _recommendationService.CosSimilarity(userVector, userId, gameVector, gameId);
			return View(result);
		}
	}
}
