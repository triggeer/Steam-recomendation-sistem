using System.Collections;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Migrations;
using WebAppTest.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebAppTest.Services.User
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
			UserProfile? userProfile = await _context.UserProfiles.FirstOrDefaultAsync(v => v.UserId == userId);
			var vector = new UserVectorResponse
			{
				TagStrength = userProfile.TagStrength,
				Length = userProfile.Length,
				GameAmount = userProfile.GameAmount,
				UpdatedAt = userProfile.UpdatedAt
			};

			return vector;
		}

		public async Task<List<OwnedGameDto>> GetUserGamesFromSteamAsync(string userId)
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

		/// <returns>список id игр и наигранного времени, не сохраняя данные в БД</returns>
		public async Task<List<OwnedGameDto>?> GetActualUserGames(string userId)
		{
			var gameData = new List<OwnedGameDto>();

			var profile = await _context.UserProfiles
				.FirstOrDefaultAsync(x => x.UserId == userId);

			DateTimeOffset weekAgo = DateTimeOffset.UtcNow.AddDays(-7);

			if (profile == null || profile.UpdatedAt <= weekAgo)
			{
				gameData = await GetUserGamesFromSteamAsync(userId);
			}
			else
			{
				gameData = await _context.UserGames
								.Where(x => x.UserId == userId)
								.Select(g => new OwnedGameDto() { AppId = g.GameId, PlayTime = g.PlayTime })
								.ToListAsync();
			}

			return gameData;
		}

		public async Task UpdateUserGames(string userId, List<OwnedGameDto> actualGames)
		{
			var dbUserGames = await _context.UserGames
				.Where(u => u.UserId == userId)
				.ToDictionaryAsync(x => x.GameId, x => x);

			foreach (var actualGame in actualGames)
			{
				if (dbUserGames.TryGetValue(actualGame.AppId, out UserGame existingGame))
				{
					if (existingGame.PlayTime != actualGame.PlayTime)
					{
						existingGame.UpdatePlayTime(actualGame.PlayTime);
					}
				}
				else
				{
					_context.UserGames.Add(new UserGame
					(	
						userId, 
						actualGame.AppId, 
						actualGame.PlayTime
					));
				}
			}

			await _context.SaveChangesAsync();

			var actualIds = actualGames
				.Select(x => x.AppId)
				.ToHashSet();

			var gamesToRemove = dbUserGames.Values
				.Where(x => !actualIds.Contains(x.GameId))
				.ToList();
				
			_context.UserGames.RemoveRange(gamesToRemove);
			
			return;
		}

		/// <summary>
		/// Получает список игр пользователя и формирует на его основе словарь тегов с их весом и числом вхождений
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public async Task<Dictionary<int, Dictionary<string, double>>> GetUserGameTags(string userId, List<OwnedGameDto> ownedGames)
		{               // RPG: {weight: 100, enterence: 3}
			int counter = 0;

			// надо id, playtime, tegs
			//List<UserGameDto> games = await GetUserGames(userId);
			List<UserGameData> games = await GetUserGames(userId, ownedGames);


			var tags = new Dictionary<int, Dictionary<string, double>>();
			foreach (UserGameData game in games)
			{
				SpyTag nonGames = new SpyTag { Name = "Utilities" };
				if (game.Tags.Any(t => t.Name == "Utilities" || t.Name == "Software"))
				{
					continue;
				}

				counter++;
				int weightConunt = 0;

				foreach (SpyTag tag in game.Tags)
				{
					weightConunt += tag.Weight;
				}

				double playtime = (double)game.PlayTime / 60;
				playtime = double.Min(playtime, 100);
				playtime = (playtime / 20);

				foreach (SpyTag tag in game.Tags)
				{
					if (game.Tags != null)
					{
						double tStrengh = ((double)tag.Weight / weightConunt) * Math.Log(1 + playtime);
						// вводим счетчик вхождений для каждого тега
						//int count = 0;
						// если тег уже есть
						if (tags.TryGetValue(tag.Id, out Dictionary<string, double> currentInnerDict))
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
							tags[tag.Id]["weight"] = currentInnerDict["weight"] + tStrengh;

							tags[tag.Id]["enterence"] = currentInnerDict["enterence"] + 1;
						}
						else
						{
							var innerDict = new Dictionary<string, double>();
							innerDict.Add("weight", tStrengh);
							innerDict.Add("enterence", 1);
							tags.Add(tag.Id, innerDict);
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

		public async Task<List<UserGameData>> GetUserGames(string userId, List<OwnedGameDto> ownedGames)
		{
			List<UserGameData> userGames = new();

			// список игр, которые есть у пользователя (id, playtime)
			//var gameData = _context.UserGames
			//						.Where(x => x.UserId == userId && ownedGames.Contains(x.GameId))
			//						.ToDictionary(x => (x.UserId, x.GameId), y => y.PlayTime);

			// steamId
			List<int> steamIds = new();
			foreach (var game in ownedGames)
				steamIds.Add(game.AppId);
				
			

			//		  steamId, DBid
			Dictionary<int, int> dbGameIds = await _context.Games
									.Select(x => new { x.Id, x.SteamAppId })
									.ToDictionaryAsync(x => x.SteamAppId, x => x.Id);

			List<int> missingIds = steamIds.Except(dbGameIds.Select(g => g.Key)).ToList();

			// для каждого id в отсутствующих id
			foreach (var id in missingIds)
			{
				// добавляем игру с таким id
				await _steamService.ImportGameAsync(id);
			}
			
			if (missingIds.Any())
			{
				dbGameIds = await _context.Games
									.Select(x => new { x.Id, x.SteamAppId })
									.ToDictionaryAsync(x => x.SteamAppId, x => x.Id);
			}

			//	 STEAM id, playtime
			Dictionary<int, int> userGameData = new Dictionary<int, int>();

			foreach (var game in ownedGames)
				userGameData.Add(dbGameIds[game.AppId], game.PlayTime);

			


			// список тегов игр пользователя (id игры, id тега, weight)
			var tagData = await _context.GameTags
									.Include(x => x.Game)
									.Include(x => x.Tag)
									.Where(x => userGameData.Keys.Contains(x.GameId))
									.ToListAsync();
			
			List<UserGameData> list = new List<UserGameData>();

			foreach (var game in userGameData)
			{
				List<SpyTag> tagList = tagData.Where(x => x.GameId == game.Key)
									.Select(x => new SpyTag
									{
										Id = x.TagId,
										Weight = x.Weight
									})
									.ToList();

				var userGame = new UserGameData()
				{ 
					AppId = game.Key,
					PlayTime = game.Value,
					Tags = tagList
				};

				userGames.Add(userGame);
			}

			return userGames;
		}


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

			foreach (SpyTag tag in game.Tags)
			{
				counter += tag.Weight;
			}

			foreach (SpyTag tag in game.Tags)
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

			foreach (SpyTag tag in game.Tags)
			{
				counter += tag.Weight;
			}

			foreach (SpyTag tag in game.Tags)
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


		/// <summary>
		/// Создаёт профиль пользователя с вектором тегов, длинной вектора, числом игр и датой обновления
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public async Task<UserProfile> CreateUserProfile(string userId)
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

			// id, playtime
			List<OwnedGameDto>? userGamesIdsPlaytime = await GetActualUserGames(userId);

			await UpdateUserGames(userId, userGamesIdsPlaytime);

			Dictionary<int, Dictionary<string, double>> tags = await GetUserGameTags(userId, userGamesIdsPlaytime); // перегруз


			//		   id,  weight
			Dictionary<int, double> userTags = new();

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
			UserProfile userProfile = await CreateUserProfile(userId);

			_context.UserProfiles.Add(userProfile);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateUserProfile(string userId)
		{
			UserProfile? oldProfile = await _context.UserProfiles.FirstOrDefaultAsync(v => v.UserId == userId);
			if (oldProfile != null )
			{
				await UpdateUserProfile(oldProfile);
			}
			return;
		}

		public async Task UpdateUserProfile(UserProfile? oldProfile)
		{
			if (oldProfile != null)
			{
				UserProfile actualProfile = await CreateUserProfile(oldProfile.UserId);
				DateTime updatedAt = DateTime.Now;
				oldProfile?.Update(actualProfile.TagStrength, actualProfile.Length, actualProfile.GameAmount, actualProfile.UpdatedAt);
				//await _context.SaveChangesAsync();
			}
			return;
		}

		public async Task<string> TransformLinkToId(string userLink)
		{
			string steamLink = $"{userLink}?xml=1";

			Stream steamResponse = await _httpClient.GetStreamAsync(steamLink);
			
			XDocument xdoc = XDocument.Load(steamResponse);

			XElement? root = xdoc.Root;

			Uri uri = new Uri(userLink);

			string userId = root.Element("steamID64").Value;

			return userId;
		}
	}
}
