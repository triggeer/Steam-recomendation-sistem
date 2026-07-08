using System.Collections;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Migrations;
using WebAppTest.Models;

namespace WebAppTest.Services
{
	public class RecommendationService : IRecommendationService
	{
		private readonly AppDbContext _context;
		private readonly HttpClient _httpClient;
		private readonly IUserService _userService;
		private readonly IDataGainService _dataGainService;
		private readonly ICreateService _createService;
		
		public RecommendationService(
		AppDbContext context,
		HttpClient httpClient,
		IUserService userService,
		IDataGainService dataGainService,
		ICreateService createService
		)
		{
			_context = context;
			_httpClient = httpClient;
			_userService = userService;
			_dataGainService = dataGainService;
			_createService = createService;
		}

		public async Task<double> CosSimilarity(
			Dictionary<string, double> userTags,
			Dictionary<string, double> gameTags)
		{
			double counter = 0;

			//// userVector = {"123123 (bogdan)": {"rpg": 0.2, "govno": 0.7}}
			//// userTags = {"rpg": 0.2, "govno": 0.7}
			//// gameTags = {"rpg": 0.3, "govno": 0.6}
			foreach (var tag in gameTags)
			{
				if (userTags.TryGetValue(tag.Key, out double userWeight))
				{
					// считаем сколярное произведение - то на сколько сильно совпадают теги пользователя и игры
					/* если 0.1 * 0.1 = 0.01
					 * если 0.01 * 0.01 = 0.0001
					 * по итогу будет типа 0.0357 если много совпадений
					 * если ничё не совпадает будет 0.0043 */
					counter += tag.Value * userWeight;
				}
			}

			// ищем длины векторов 
			/*
			 * 0.10² = 0.0100
				0.08² = 0.0064
				0.12² = 0.0144
				0.05² = 0.0025
				0.02² = 0.0004
				= 0.0337
				√0.0337 = 0.1836
			 */

			double userNorm =
				Math.Sqrt(userTags.Values.Sum(v => v * v));

			double gameNorm =
				Math.Sqrt(gameTags.Values.Sum(v => v * v));

			if (userNorm == 0 || gameNorm == 0)
				return 0;

			// возвращаем Косинусное сходство векторов (0.3)
			return counter / (userNorm * gameNorm);
		}

		//public async Task GetUserVector()

		public async Task<UserVectorResponse> FormUserTagVector(string userId)
		{
			// проверяем, есть ли уже сформированный вектор
			bool exists = await _dataGainService.ChekUserTagVectorExistense(userId);
			
			// если нет - добавляем
			if (!exists)
			{
				await _userService.AddUserTagVector(userId);
			}

			int gameAmount = await _dataGainService.GetCurrentGameAmount(userId);
			DateTime currentDate = DateTime.Now;
			// берем инфу из бд
			UserVectorResponse vector = await _userService.GetUserVector(userId);

			//var vector = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId);
			if (vector.GameAmount != gameAmount || (currentDate - vector.UpdatedAt) > TimeSpan.FromDays(7))
			{

				await _userService.UpdateUserVector(userId);
			}


			return vector; // гуд
		}

		private async Task<Dictionary<string, double>> FormUserTagsDict(string userId)
		{
			UserVectorResponse userVector = await FormUserTagVector(userId);
			/*
			 * {
				  "2D": 0.31809245060735336,
				  "3D": 0.09755129007135584,
				  "4X": 0.007433650640747398,
				  "VR": 0.062164114246365025,
				  "Elf": 0.010591989430834014,
				  "FMV": 0.013916809731422241,
				  "FPS": 0.21931425216181996,
				  "PvE": 0.08735315933855649,
				  "PvP": 0.1670017616192241,
				  "RPG": 0.3379300102282034,
				  ...
				}
			 */
			Dictionary<string, double> userTags = userVector.TagStrength;
			return userTags;
		}

		//public async Task<List<RecommendationDto>> FormRecommendationListAsync(string userId)
		//{
		//	var list = new List<RecommendationDto>();
		//	var userVector = await FormUserTagVector(userId);
		//	/*
		//	 * {
		//		  "2D": 0.31809245060735336,
		//		  "3D": 0.09755129007135584,
		//		  "4X": 0.007433650640747398,
		//		  "VR": 0.062164114246365025,
		//		  "Elf": 0.010591989430834014,
		//		  "FMV": 0.013916809731422241,
		//		  "FPS": 0.21931425216181996,
		//		  "PvE": 0.08735315933855649,
		//		  "PvP": 0.1670017616192241,
		//		  "RPG": 0.3379300102282034,
		//		  ...
		//		}
		//	 */
		//	Dictionary<string, double> userTags = userVector.TagStrength;
		//	/* получаем все id игр из БД
		//	 * для каждого id берем игру из БД
		//	 */

		//	var userGames = await _userService.GetUserGamesIdList(userId);
		//	var userIds = userGames.Select(x => x.AppId).ToList();

		//	var games = await _context.Games.Select(
		//		g => new 
		//		{ 
		//			g.SteamAppId, 
		//			g.Name, 
		//			g.Owners,
		//			g.UserScore,
		//			g.ReviewAmount,
		//			g.GameTags
		//		}).ToListAsync();

		//	foreach (var game in games)
		//	{
		//		if (!userIds.Contains(game.SteamAppId))
		//		{
		//			var gameVector = await _userService.GetGameTagsStrengh(game.SteamAppId);
		//			Dictionary<string, double> gameTags = gameVector[game.SteamAppId.ToString()];
		//			double result = await CosSimilarity(userTags, gameTags);
		//			/*
		//				Monster Hunter: World: --> 0,5835905493661762 <---
		//				ELDEN RING NIGHTREIGN: --> 0,5826818525389187 <--- 
		//				Valheim: --> 0,5805596998270341 <---
		//			*/
		//			int reviewAmount = game.ReviewAmount;
		//			double score = game.UserScore;
		//			// минимальное количество голосов, необходимое для того, чтобы рейтинг стал значимым
		//			int min = 50000;
		//			// средний рейтинг среди всех объектов в базе
		//			double avgScore = 0.5;

		//			double rating = ((reviewAmount * score) + (min * avgScore)) / (reviewAmount + min);

		//			result *= rating;

		//			var tags = new List<TagFromDb>();

		//			foreach (var tag in game.GameTags)
		//			{
		//				var newTag = new TagFromDb { Id = tag.TagId, Name = tag.Tag.Name};
		//				tags.Add(newTag);
		//			}

		//			if (result > 0)
		//			{
		//				list.Add(new RecommendationDto
		//				{
		//					GameId = game.SteamAppId,
		//					GameName = game.Name,
		//					CosSimilarity = result,
		//					GameTags = tags
		//				});
		//			}

		//		}
		//	}
		//	var sortedList = list.OrderByDescending(r => r.CosSimilarity).ToList();


		//	return sortedList;
		//}

		public async Task<List<RecommendationDto>> FormRecommendationListAsync(string userId)
		{
			List<RecommendationDto> list = await FormUnsortedRecommendationList(userId);

			// сортируем по рейтингу схожести
			List<RecommendationDto> sortedList = list.OrderByDescending(r => r.CosSimilarity).ToList();

			return sortedList;
		}

		//public async Task<List<RecommendationDto>> FormRecomendationsOnTagAsync(string userId, int tagId)
		//{
		//	var list = new List<RecommendationDto>();
		//	var recomendations = await FormRecommendationListAsync(userId);
		//	foreach (var game in recomendations)
		//	{
		//		if (game.GameTags.Any(x => x.Id == tagId))
		//		{
		//			list.Add(game);
		//		}
		//	}

		//	return list;
		//}

		public async Task<RecommendationsOnTeg> FormRecomendationsOnTagAsync(string userId, int tagId)
		{
			var list = new List<RecommendationDto>();
			var recomendations = await FormRecommendationListAsync(userId);
			foreach (var game in recomendations)
			{
				if (game.GameTags.Any(x => x.Id == tagId))
				{
					list.Add(game);
				}
			}
			var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == tagId);

			var result = new RecommendationsOnTeg{ RecList = list, tag = tag.Name};

			return result;
		}

		public async Task<List<RecommendationDto>> ForUniqueRecomendationsAsync(string userId)
		{
			var uniqueList = new List<RecommendationDto>();
			List<RecommendationDto> recGames = await FormUnsortedRecommendationList(userId);
			
			var userTags = await FormUserTagsDict(userId);

			var sortedTags = userTags.OrderByDescending(x => x.Value).ToList();

			foreach (var tag in sortedTags)
			{
				foreach (var game in recGames)
				{
					if ( (!uniqueList.Contains(game)) && (game.GameTags.Any(t => t.Name == tag.Key)) )
					{
						uniqueList.Add(game);
						break;
					}
				}
			}

			return uniqueList;

		}

		private async Task<RecommendationDto> CalculateRecomendationRating(Dictionary<string, double> userTags, Game game)
		{
			var gameVector = await _userService.GetGameTagsStrengh(game.SteamAppId);
			Dictionary<string, double> gameTags = gameVector[game.SteamAppId.ToString()];
			double similarity = await CosSimilarity(userTags, gameTags);
			/*
				Monster Hunter: World: --> 0,5835905493661762 <---
				ELDEN RING NIGHTREIGN: --> 0,5826818525389187 <--- 
				Valheim: --> 0,5805596998270341 <---
			*/
			int reviewAmount = game.ReviewAmount;
			double score = game.UserScore;

			double rating = CalculateBayesRaiting(reviewAmount, score);

			similarity *= rating;

			var tags = new List<TagFromDb>();

			foreach (var tag in game.GameTags)
			{
				var newTag = new TagFromDb { Id = tag.TagId, Name = tag.Tag.Name };
				tags.Add(newTag);
			}

			RecommendationDto recommendation = new RecommendationDto
			{
				GameId = game.SteamAppId,
				GameName = game.Name,
				CosSimilarity = similarity,
				GameTags = tags
			};

			return recommendation;
		}

		private static double CalculateBayesRaiting(int reviewAmount, double score)
		{
			// минимальное количество голосов, необходимое для того, чтобы рейтинг стал значимым
			const int min = 50000;
			// средний рейтинг для всех объектов в базе
			const double avgScore = 0.5;

			double rating = ((reviewAmount * score) + (min * avgScore)) / (reviewAmount + min);
			return rating;
		}

		private async Task<List<RecommendationDto>> FormUnsortedRecommendationList(string userId)
		{
			var list = new List<RecommendationDto>();
			/*
			 * {
				  "2D": 0.31809245060735336,
				  "3D": 0.09755129007135584,
				  "4X": 0.007433650640747398,
				  "VR": 0.062164114246365025,
				  "Elf": 0.010591989430834014,
				  "FMV": 0.013916809731422241,
				  "FPS": 0.21931425216181996,
				  "PvE": 0.08735315933855649,
				  "PvP": 0.1670017616192241,
				  "RPG": 0.3379300102282034,
				  ...
				}
			 */
			Dictionary<string, double> userTags = await FormUserTagsDict(userId);
			/* получаем все id игр из БД
			 * для каждого id берем игру из БД
			 */

			List<OwnedGameDto> userGames = await _userService.GetUserGamesIdList(userId);
			List<int> userIds = userGames.Select(x => x.AppId).ToList();

			var games = await _context.Games.ToListAsync();

			foreach (var game in games)
			{
				if (!userIds.Contains(game.SteamAppId))
				{
					RecommendationDto recommendation = await CalculateRecomendationRating(userTags, game);

					if (recommendation.CosSimilarity > 0)
					{
						list.Add(recommendation);
					}
				}
			}

			return list;
		}

	}
}
