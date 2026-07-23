using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Models;

namespace WebAppTest.Services
{
	public class UserService : IUserService
	{
		private readonly AppDbContext _context;
		private readonly HttpClient _httpClient;
		private readonly ISteamService _steamService;
		private readonly IDataGainService _dataGainService;
		private readonly IConfiguration _configuration;

		public UserService(
		AppDbContext context, 
		HttpClient httpClient, 
		ISteamService steamService, 
		IDataGainService dataGainService,
		IConfiguration configuration)
		{
			_context = context;
			_httpClient = httpClient;
			_steamService = steamService;
			_dataGainService = dataGainService;
			_configuration = configuration;
		}

		public async Task<UserVectorResponse?> GetUserVector(string userId)
		{
			UserProfile? userProfile = await _context.UserProfiles.FirstOrDefaultAsync(v => v.Id == userId);
			var vector = new UserVectorResponse
			{
				TagStrength = userProfile.TagStrength,
				Length = userProfile.Length,
				GameAmount = userProfile.GameAmount,
				UpdatedAt = userProfile.UpdatedAt
			};

			return vector;
		}

		public async Task<List<OwnedGameDto>> GetUserGamesIdList(string userId)
		{
			var apiKey = _configuration.GetValue<string>("Steam:ApiKey");
			var url = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={apiKey}&steamid={userId}&format=json";

			var userGameList = new List<Game>();

			var steamResponse = await _httpClient.GetAsync(url);
			steamResponse.EnsureSuccessStatusCode();

			var steamJson = await steamResponse.Content.ReadAsStringAsync();
			var steamData = JsonSerializer.Deserialize<
			Dictionary<string, UserGamesResponse>
			>(steamJson);

			List<OwnedGameDto> ownedGames = steamData["response"].games;
			return ownedGames; // (id, часы)
		}

		public async Task<List<UserGameDto>> GetUserGames(string userId)
		{
			// получили спсиок (id, часы)
			List<OwnedGameDto> list = await GetUserGamesIdList(userId);

			// получаем только список id (все игры(id) пользователя)
			var idList = list.Select(x => x.AppId).ToList();

			// возвращаем список игр по id (которые сейчас есть в БД)
			var dbGames = await _steamService.GetGameList(idList);

			// создаем список id игр пользователя (idList), которых ещё нет в БД (dbGames)
			var missingIds = idList.Except(dbGames.Select(g => g.SteamAppId)).ToList();

			// для каждого id в отсутствующих id
			foreach (var id in missingIds)
			{
				// добавляем игру с таким id
				await _steamService.ImportGameAsync(id);
			}

			// обновляем данные в случае если нашлись отсутствующие в БД id
			if (missingIds != null)
			{
				dbGames = await _steamService.GetGameList(idList);
			}

			// вводим словарь для "быстрого поиска"
			var gameDictionary = dbGames.ToDictionary(g => g.SteamAppId);

			// вводим список ИГОР (ТАМ ИМЯ, ЧАСЫ, ТЕГИ И ВСЯ ХЕРНЯ)
			var games = new List<UserGameDto>();

			// для каждого элемента из списка с id и минутами в игре
			foreach (OwnedGameDto gameObj in list)
			{

				/* 
				 * если в словаре нет такого id -> добавляем игру с таким id
					если в словаре нет 
					попытаться найти игру, у которой id это gameObj.AppId
					результат закладываем в переменную game
						если игра с таким id есть - тогда game = эта игра
						если нет, тогда game = null

						*/
				//Game? game = gameDictionary[gameObj.AppId];
				if (!gameDictionary.TryGetValue(gameObj.AppId, out var game))
				{
					await _steamService.ImportGameAsync(gameObj.AppId); continue;
				}

				games.Add(new UserGameDto
				{
					AppId = gameObj.AppId,
					PlayTime = gameObj.PlayTime,
					Name = game.Name,
					Genres = game.GameGenres.Select(x => x.Genre.Name).ToList(),
					Tags = game.GameTags.Select(x => new SpyTagDto
					{
						Name = x.Tag.Name,
						Weight = x.Weight
					}).ToList(),
				});
			}
			return games;
		}

		//public async Task<Dictionary<string, int>> GetUserGameTags(string userId)
		//{
		//	List<UserGameDto> games = await GetUserGames(userId);
		//	var tags = new Dictionary<string, int>();
		//	foreach (UserGameDto game in games)
		//	{
		//		foreach (SpyTagDto tag in game.Tags)
		//		{
		//			if (game.Tags != null)
		//			{
		//				// вводим счетчик вхождений для каждого тега
		//				//int count = 0;
		//				// если тег уже есть
		//				if (tags.TryGetValue(tag.Name, out int currentValue))
		//				{
		//					tags[tag.Name] = currentValue + 1;
		//				}
		//				else
		//				{
		//					tags.Add(tag.Name, 1);
		//				}
		//			}
		//			else continue;
		//		}
		//	}

		//	var sortTags = tags.OrderByDescending(pair => pair.Value).ToDictionary(p => p.Key, p => p.Value);

		//	return sortTags;
		//}

		//public async Task FormTagPrefList(string userId)
		//{
		//	// в словаре будут Тег: значимость 
		//	var preference = new Dictionary<string, int>();
		//	Dictionary<string, int> tagAmount = await GetUserGameTags(userId);

		//}


		/// ///////////////


		public async Task<Dictionary<string, Dictionary<string, double>>> GetUserGameTags(string userId)
		{               // RPG: {weight: 100, enterence: 3}
			int counter = 0;
			List<UserGameDto> games = await GetUserGames(userId);
			var tags = new Dictionary<string, Dictionary<string, double>>();
			foreach (UserGameDto game in games)
			{
				SpyTagDto nonGames = new SpyTagDto { Name = "Utilities" };
				if (game.Tags.Any(t => t.Name == "Utilities" || t.Name == "Software"))
				{
					continue;
				}
				counter++;
				int weightConunt = 0;
				foreach (SpyTagDto tag in game.Tags)
				{
					weightConunt += tag.Weight;
				}
				double playtime = (double)game.PlayTime / 60;
				playtime = double.Min(playtime, 100);
				playtime = (playtime / 20);

				foreach (SpyTagDto tag in game.Tags)
				{
					if (game.Tags != null)
					{
						double tStrengh = ((double)tag.Weight / weightConunt) * Math.Log(1 + playtime);
						// вводим счетчик вхождений для каждого тега
						//int count = 0;
						// если тег уже есть
						if (tags.TryGetValue(tag.Name, out Dictionary<string, double> currentInnerDict))
						{   //	RPG:	  {weight: 100, ent: 4}
							/*11446
							 * mk x = 0,7172449274855845
							 * jojo = 0,4655811906556142
								* всего 20 тегов
								* "tags": {
									"Souls-like": 9604, 0.11
									"Dark Fantasy": 7905, 0.09
									"Difficult": 6967, 0.08
									"RPG": 5966, 0.07
									"Atmospheric": 5311, 0.06
									"Lore-Rich": 4544, 0.05
									"Third Person": 4272, 0.05
									"Exploration": 3797, 0.045
									"Story Rich": 3779, 0.045 
									"Action RPG": 3576, 0.04
									"Co-op": 3499, 0.04
									"Great Soundtrack": 3418, 0.04
									"Adventure": 3307, 0.04
									"Action": 3292, 0.04
									"Multiplayer": 3214, 0.04
									"PvP": 3161, 0.03
									"Open World": 3080 0.03,
									"Singleplayer": 2427, 0.025
									"Character Customization": 1936, 0.02 
									"Replay Value": 1926 0.02
									}
									86000
									для каждого тега вес / сумму всесов = релевантность (сила) тега
									если сила тега = +-0.1 - тег очень релевантный
									если ~ (0.4, 0.7) норм 
									если < 0.03 не очень релевантный 
									=>>>>> вычисляем силу тега и записываем её вместо всеа
								*/
							tags[tag.Name]["weight"] = currentInnerDict["weight"] + tStrengh;

							tags[tag.Name]["enterence"] = currentInnerDict["enterence"] + 1;
						}
						else
						{
							var innerDict = new Dictionary<string, double>();
							innerDict.Add("weight", tStrengh);
							innerDict.Add("enterence", 1);
							tags.Add(tag.Name, innerDict);
						}

					}
					else continue;
				}
			}

			//var sortTags = tags.OrderByDescending(pair => pair.Value).ToDictionary(p => p.Key, p => p.Value);
			foreach (var tag in tags)
			{
				//Dictionary<string, Dictionary<string, double>>
				//string, Dictionary<string, double>
				var currentWeight = tag.Value["weight"];
				tag.Value["weight"] = currentWeight / (Math.Log(tag.Value["enterence"]) + 1);
			}

			return tags;
		}

		
		/// ??????
		//public async Task<Dictionary<string, Dictionary<string, double>>> FormGameTagVector(int gameId)
		//{
		//	/*
		//	 * DS3 = {
		//			RPG: 0.82,
		//			OpenWorld: 0.65,
		//			Fantasy: 0.54,
		//			SoulsLike: 0.71,
		//			StoryRich: 0.33,
		//			PvP: 0.05
		//		}
		//	 */
		//	var vector = new Dictionary<string, Dictionary<string, double>>();
		//	bool exists = await _dataGainService.CheckGameExistense(gameId);
		//	if (!exists)
		//	{
		//		await _steamService.ImportGameAsync(gameId);	
		//	}

		//	return vector;
		//}
		//							"123 (ds3)": {rpg:0.5, sols-like:0.7}
		public async Task<Dictionary<string, Dictionary<string, double>>> GetGameTagsStrengh(int gameId)
		{
			bool exists = await _dataGainService.CheckGameExistense(gameId);
			
			if (!exists)
			{
				await _steamService.ImportGameAsync(gameId);
			}

			GameResponse game = await _steamService.GetGame(gameId);

			if (game == null)
			{
				return null;
			}
			var vector = new Dictionary<string, Dictionary<string, double>> { [gameId.ToString()] = [] };

			int counter = 0;

			foreach (SpyTagDto tag in game.Tags)
			{
				counter += tag.Weight;
			}

			foreach (SpyTagDto tag in game.Tags)
			{
				if (game.Tags != null)
				{
					double tStrengh = (double)tag.Weight / counter;
					//				 "123 (ds3)":			 {rpg:			0.5,...}	
					if (vector[gameId.ToString()].TryGetValue(tag.Name, out double currentValue))
					{
						vector[gameId.ToString()][tag.Name] = currentValue + tStrengh;
					}
					else
					{//		        "123 (ds3)":	  {rpg:		0.5,...}	
						vector[gameId.ToString()].Add(tag.Name, tStrengh);
					}
				}
			}

			return vector;
		}

		public async Task<Dictionary<string, double>> GetGameTagsStrengh1(int gameId)
		{
			bool exists = await _dataGainService.CheckGameExistense(gameId);

			if (!exists)
			{
				await _steamService.ImportGameAsync(gameId);
			}

			GameResponse game = await _steamService.GetGame(gameId);

			if (game == null)
			{
				return null;
			}
			var vector = new Dictionary<string, double>();

			int counter = 0;

			foreach (SpyTagDto tag in game.Tags)
			{
				counter += tag.Weight;
			}

			foreach (SpyTagDto tag in game.Tags)
			{
				if (game.Tags == null)
					return new Dictionary<string, double>();
					
				

				double tStrengh = (double)tag.Weight / counter;
				//				 "123 (ds3)":			 {rpg:			0.5,...}	
				if (vector.TryGetValue(tag.Name, out double currentValue))
				{
					vector[tag.Name] = currentValue + tStrengh;
				}
				else
				{//		        "123 (ds3)":	  {rpg:		0.5,...}	
					vector.Add(tag.Name, tStrengh);
				}
			}

			return vector;
		}

		//public async Task<Dictionary<string, double>> GetGameTagsStrengh2(int gameId)
		//{
		//	bool exists = await _dataGainService.CheckGameExistense(gameId);

		//	if (!exists)
		//	{
		//		await _steamService.ImportGameAsync(gameId);
		//	}

		//	GameResponse game = await _steamService.GetGame(gameId);

		//	if (game == null)
		//	{
		//		return null;
		//	}
		//	var vector = new Dictionary<string, double>();

		//	int counter = 0;

		//	foreach (SpyTagDto tag in game.Tags)
		//	{
		//		counter += tag.Weight;
		//	}

		//	foreach (SpyTagDto tag in game.Tags)
		//	{
		//		if (game.Tags != null)
		//		{
		//			double tStrengh = (double)tag.Weight / counter;
		//			//				 "123 (ds3)":			 {rpg:			0.5,...}	
		//			if (vector.TryGetValue(tag.Name, out double currentValue))
		//			{
		//				vector[tag.Name] = currentValue + tStrengh;
		//			}
		//			else
		//			{//		        "123 (ds3)":	  {rpg:		0.5,...}	
		//				vector.Add(tag.Name, tStrengh);
		//			}
		//		}
		//	}

		//	return vector;
		//}


		public async Task<UserProfile> CreateUserVector(string userId)
		{
			/*
		 	 * U = {
					RPG: 0.82,
					OpenWorld: 0.65,
					Fantasy: 0.54,
					SoulsLike: 0.71,
					StoryRich: 0.33,
					PvP: 0.05
				}
		 	 */
			var tags = await GetUserGameTags(userId);

			Dictionary<string, double> userTags = new();

			foreach (var tag in tags)
			{
				userTags.Add(tag.Key, tag.Value["weight"]);
			}

			double userLength =
				Math.Sqrt(userTags.Values.Sum(v => v * v));

			int gameAmount = await _dataGainService.GetCurrentGameAmount(userId);

			DateTime updatedAt = DateTime.Now;

			var userProfile = new UserProfile(userId, userTags, userLength, gameAmount, updatedAt);
			return userProfile;
		}

		public async Task AddUserTagVector(string userId)
		{/*
		 	 * U = {
					RPG: 0.82,
					OpenWorld: 0.65,
					Fantasy: 0.54,
					SoulsLike: 0.71,
					StoryRich: 0.33,
					PvP: 0.05
				}
		 	 */
			var userProfile = await CreateUserVector(userId);

			_context.UserProfiles.Add(userProfile);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateUserVector(string userId)
		{
			UserProfile? oldVector = await _context.UserProfiles.FirstOrDefaultAsync(v => v.Id == userId);
			var newVector = await CreateUserVector(userId);
			DateTime updatedAt = DateTime.Now;
			oldVector.Update(userId, newVector.TagStrength, newVector.Length, newVector.GameAmount, newVector.UpdatedAt);
			await _context.SaveChangesAsync();
			return;
		}
	}
}

/* "RPG": {
 * "weight": 0.7, 
 * "enterence": 3
 * },
 * "MOBA" {
 * "weight": 0.3,
 * "enterence": 5
 * }
 */