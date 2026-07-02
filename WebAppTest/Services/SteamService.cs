using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Validation;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Models;
using WebAppTest.Services;
using static System.Net.WebRequestMethods;

namespace WebAppTest.Services
{
	public class SteamService : ISteamService
	{
		private readonly AppDbContext _context;
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private readonly IDataGainService _dataGainService;
		private readonly ICreateService _createService;

		public SteamService(
			HttpClient httpClient,
			IConfiguration configuration,
			AppDbContext context,
			IDataGainService dataGainService,
			ICreateService createService)
		{
			_httpClient = httpClient;
			_configuration = configuration;
			_context = context;
			_dataGainService = dataGainService;
			_createService = createService;
		}

		//public async Task<SteamGameDto?> GameDetailsAsync(int appId)
		//{
		//	// указали url
		//	var url =
		//		$"https://store.steampowered.com/api/appdetails?appids={appId}&l=russian";

		//	// делаем запрос и ждем ответ
		//	var response = await _httpClient.GetAsync(url);

		//	// проверяем что без ошибок, иначе выкидываем ексептион
		//	response.EnsureSuccessStatusCode();

		//	// читаем JSON как строку (теперь просто биг строка)
		//	var json = await response.Content.ReadAsStringAsync();

		//	// не учитываем регистр
		//	var options = new JsonSerializerOptions
		//	{
		//		PropertyNameCaseInsensitive = true
		//	};

		//	// превращаем JSON в словарь с id в качестве ключа и
		//	var data = JsonSerializer.Deserialize<
		//		Dictionary<string, SteamStoreResponse>
		//	>(json, options);

		//	// берём объект с таким-то id и возвращаем его data
		//	return data?[appId.ToString()]?.data;
		//} 

		public async Task<GameResponse?> GetGame(int appId)
		{
			Game? game = await _context.Games
			.Include(gg => gg.GameGenres).ThenInclude(g => g.Genre)
			.Include(gt => gt.GameTags).ThenInclude(t => t.Tag)
			.FirstOrDefaultAsync(g => g.SteamAppId == appId);

			if (game == null)
			{
				return null;
			}

			var details = new GameResponse
			{
				Name = game.Name,
				Genres = game.GameGenres.Select(gg => gg.Genre.Name).ToList(),

				Tags = game.GameTags.Select(gt => new SpyTagDto
				{
					Name = gt.Tag.Name,
					Weight = gt.Weight
				})
				.ToList(),
				DetailedDescription = game.DetailedDescription,
				UserScore = game.UserScore,
				Owners = game.Owners,
				InitialPrice = game.InitialPrice
			};
			return details;
		}

		public async Task<List<Game>?> GetGameList(List<int> appIds)
		{
			return await _context.Games
								.Where(g => appIds.Contains(g.SteamAppId))
								.Include(g => g.GameGenres)
									.ThenInclude(gg => gg.Genre)
								.Include(g => g.GameTags)
									.ThenInclude(gt => gt.Tag)
								.ToListAsync();
		}

		/* метод async потаму-что надо ждать, а Task значит выполнение работы метода без возврата чего либо
												+ Task а не void т.к. Task можно await
		*/
		public async Task ImportGameAsync(int appId)
		{
			bool exitsts = await _dataGainService.CheckGameExistense(appId);
			if (!exitsts)
			{
				SteamGameDto steamDto = await _dataGainService.GetSteamData(appId);
				if (steamDto != null)
				{
					SpyGameDto spyDto = await _dataGainService.GetSpyData(appId);

					Dictionary<string, int> tags = await _dataGainService.GetTags(appId);

					(double userScore, int reviewAmount) = await _dataGainService.GetUserScore(appId);

					long owners = await _dataGainService.GetOwners(appId);

					int initialPrice = await _dataGainService.GetInitPrice(appId);

					List<string> genres = await _dataGainService.GetGenres(appId);

					Game game = await _createService.GameCreate(
					appId, steamDto.Name, steamDto.DetailedDescription,
					userScore, reviewAmount, owners,
					initialPrice, genres, tags);

					await _createService.AddGame(game);
				}
				else return;
			}
			else return;
		}

		public async Task UpdateGame(int appId)
		{
			/* СМОТРИМ ДАННЫЕ ПО ID ИГРЫ 
			 *		
			 * ЕСЛИ В БД НЕТ ТАКОГО ID -> ДОБАВЛЯЕМ ИГРУ 
			 * ЕСЛИ В БД ЕСТЬ ИГРА С ТАКИМ ID -> СМОТРИМ, ТЕ ЖЕ ДАННЫЕ ИЛИ НЕТ
			 * ЕСЛИ ТЕ ЖЕ -> СКИП
			 * ЕСЛИ ДРУГИЕ -> ОБНОВЛЯЕМ
			 */

			var exists = await _dataGainService.CheckGameExistense(appId);
			if (!exists)
			{
				await ImportGameAsync(appId);
				return;
			}
			else
			{
				var oldGame = await _context.Games
									.FirstOrDefaultAsync(g => g.SteamAppId == appId);


				var newInitialPrice = await _dataGainService.GetInitPrice(appId);


				if (oldGame.InitialPrice != newInitialPrice)
				{
					oldGame.UpdateInitialPrice(newInitialPrice);
					await _context.SaveChangesAsync();
				}
				else return;
			}
		}
	}
}

//public async Task<Dictionary<string, double>> FormRecommendationListAsync(string userId)
//{
//	var list = new Dictionary<string, double>();
//	var userVector = await FormUserTagVector(userId);
//	Dictionary<string, double> userTags = userVector.TagStrength;
//	/* получаем все id игр из БД
//	 * для каждого id берем игру из БД
//	 */
//	var idList = await _context.Games.Select(g => g.SteamAppId).ToListAsync();
//	foreach (var gameId in idList)
//	{
//		var game = await _context.Games.FirstOrDefaultAsync(g => g.SteamAppId == gameId);
//		var gameVector = await _userService.GetGameTagsStrengh(gameId);
//		Dictionary<string, double> gameTags = gameVector[gameId.ToString()];
//		double result = await CosSimilarity(userTags, gameTags);
//		list.Add(game.Name, result);
//	}
//	Dictionary<string, double> sortedDict = list
//	.OrderByDescending(pair => pair.Value)
//	.ToDictionary(pair => pair.Key, pair => pair.Value);
//	return sortedDict;
//}