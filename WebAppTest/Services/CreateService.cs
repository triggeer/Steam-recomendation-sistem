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
			string? currency,
			double vectorLength, 
			List<string> genres, 
			Dictionary<string, int> tags
			)
		{
			var game = new Game(appId, name, imgUrl, detaildDescription, userScore, reviewAmount, owners, initialPrice, finalPrice, currency, vectorLength);

			foreach (string genreName in genres)
			{
				Genre? existingGenre = await _context.Genres.FirstOrDefaultAsync(g => g.Name == genreName);

				if (existingGenre == null)
				{
					existingGenre = new Genre(genreName);
					_context.Genres.Add(existingGenre);
				}
				
				game.GameGenres.Add(new GameGenre(game, existingGenre));
			}

			List<GameTag> tagList = new();
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

				GameTag tag = new GameTag(game, existingTag, tagWeight, strength);
				tagList.Add(tag);
				game.GameTags.Add(tag);
			}

			double vectorLen = CalculateVectorLen(tagList);
			game.UpdateVectorLength(vectorLen);

			return game;
		}

		public async Task AddGame(Game game)
		{
			_context.Games.Add(game);
			await _context.SaveChangesAsync();
		}

		public double CalculateVectorLen(List<GameTag> gameTags)
		{
			double length = 0;
			foreach (var tags in gameTags.GroupBy(x => x.GameId))
			{
				int sum = tags.Sum(x => x.Weight);

				if (sum == 0)
					continue;

				double sqrSum = 0;

				foreach (var tag in tags)
					sqrSum += tag.Strength * tag.Strength;
				
				length = Math.Sqrt(sqrSum);
			}
			return length;
		}

	}
}
