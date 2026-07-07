using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Models;
namespace WebAppTest.Services
{
	public class CreateService : ICreateService
	{
		private readonly AppDbContext _context;
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private readonly IDataGainService _dataGainService;
		

		public CreateService(
			HttpClient httpClient,
			IConfiguration configuration,
			AppDbContext context,
			IDataGainService dataGainService
			)
		{
			_httpClient = httpClient;
			_configuration = configuration;
			_context = context;
			_dataGainService = dataGainService;
		}

		public async Task<Game> GameCreate(int appId, string name, string detaildDescription, double userScore, int reviewAmount, long owners, int? initialPrice, List<string> genres, Dictionary<string, int> tags)
		{
			var game = new Game(appId, name, detaildDescription, userScore, reviewAmount, owners, initialPrice);

			foreach (string genreName in genres)
			{
				// проверяем, есть ли уже такой жанр в БД (если название жанра в списке genres совпадает с названием в БД)
				Genre? existingGenre = await _context.Genres.FirstOrDefaultAsync(g => g.Name == genreName);

				// если нет, то добавляем в БД
				if (existingGenre == null)
				{
					existingGenre = new Genre(genreName);
					_context.Genres.Add(existingGenre);
				}

				// Добавляем связь игры и жанра
				game.GameGenres.Add(new GameGenre
				{
					Game = game,
					Genre = existingGenre
				}
				);
			}

			foreach (KeyValuePair<string, int> tagPair in tags)
			{
				string tagName = tagPair.Key;

				int tagWeight = tagPair.Value;

				Tag? existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);

				if (existingTag == null)
				{
					existingTag = new Tag(tagName);

					_context.Tags.Add(existingTag);
				}

				game.GameTags.Add(new GameTag
				{
					Game = game,
					Tag = existingTag,
					Weight = tagWeight
				});
			}

			return game;
		}

		public async Task AddGame(Game game)
		{
			_context.Games.Add(game);
			await _context.SaveChangesAsync();
		}


		//public async Task AddUserTagVector(string userId)
		//{/*
		// 	 * U = {
		//			RPG: 0.82,
		//			OpenWorld: 0.65,
		//			Fantasy: 0.54,
		//			SoulsLike: 0.71,
		//			StoryRich: 0.33,
		//			PvP: 0.05
		//		}
		// 	 */
		//	//var vector = new Dictionary<string, Dictionary<string, double>> { [userId] = [] };

		//	var exist = await _dataGainService.ChekUserTagVectorExistense(userId);

		//	if (exist)
		//	{
		//		return;
		//	}

		//	var tags = await _userService.GetUserGameTags(userId);
		//	//foreach (var tag in tags)
		//	//{
		//	//	vector[userId].Add(tag.Key, tag.Value["weight"]);
		//	//}

		//	// {"rpg": 0.07, "shooter": 0.02}
		//	Dictionary<string, double> userTags = new();

		//	foreach (var tag in tags)
		//	{
		//		userTags.Add(tag.Key, tag.Value["weight"]);
		//	}

		//	double userLength =
		//		Math.Sqrt(userTags.Values.Sum(v => v * v));


		//	// полчуаем число игр через steam Api
		//	var apiKey = _configuration.GetValue<string>("Steam:ApiKey");
		//	string url = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={apiKey}&steamid={userId}&format=json";


		//	var steamRespone = await _httpClient.GetAsync(url);
		//	steamRespone.EnsureSuccessStatusCode();
		//	var steamJson = await steamRespone.Content.ReadAsStringAsync();
		//	var data = JsonSerializer.Deserialize<
		//	Dictionary<string, UserGamesResponse>
		//	>(steamJson);

		//	int gameAmount = data["response"].game_count;

		//	var userProfile = new UserProfile(userId, userTags, userLength, gameAmount);

		//	_context.UserProfiles.Add(userProfile);
		//	await _context.SaveChangesAsync();
		//}
	}
}
