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

		public async Task<Game> GameCreate(
			int appId, 
			string name, 
			string imgUrl,
			string detaildDescription, 
			double userScore, 
			int reviewAmount, 
			long owners, 
			int? initialPrice, 
			int? finalPrice, 
			double vectorLength, 
			List<string> genres, 
			Dictionary<string, int> tags
			)
		{
			var game = new Game(appId, name, imgUrl, detaildDescription, userScore, reviewAmount, owners, initialPrice, finalPrice, vectorLength);

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
				game.GameGenres.Add(new GameGenre(game, existingGenre));
			}

			foreach (KeyValuePair<string, int> tagPair in tags)
			{
				int sum = tags.Values.Sum();	

				string tagName = tagPair.Key;

				int tagWeight = tagPair.Value;

				double strength = (double)tagPair.Value / sum;

				Tag? existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);

				if (existingTag == null)
				{
					existingTag = new Tag(tagName);

					_context.Tags.Add(existingTag);
				}

				game.GameTags.Add(new GameTag(game, existingTag, tagWeight, strength));
			}

			return game;
		}

		public async Task AddGame(Game game)
		{
			_context.Games.Add(game);
			await _context.SaveChangesAsync();
		}

	}
}
