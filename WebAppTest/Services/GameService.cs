using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Migrations;
using WebAppTest.Models;

namespace WebAppTest.Services
{
	public class GameService : IGameService
	{
		private readonly AppDbContext _context;
		private readonly IDataGainService _dataGainService;

		public GameService(
			IConfiguration configuration,
			AppDbContext context,
			IDataGainService dataGainService
			)
		{
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
			Dictionary<string, int> tags
			)
		{
			var game = new Game(appId, name, imgUrl, detaildDescription, userScore, reviewAmount, owners, initialPrice, finalPrice, currency, vectorLength);

			int sum = tags.Values.Sum();

			List<GameTag> tagList = new();
			foreach (KeyValuePair<string, int> tagPair in tags)
			{
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

		public double CalculateVectorLen(List<GameTag> tags)
		{
			double length = 0;

			int sum = tags.Sum(x => x.Weight);

			if (sum == 0)
				return length;

			double sqrSum = 0;

			foreach (var tag in tags)
				sqrSum += tag.Strength * tag.Strength;

			length = Math.Sqrt(sqrSum);
			
			return length;
		}

		public async Task<bool> ImportGameAsync(int appId)
		{
			bool exitsts = await _context.Games.AnyAsync(g => g.SteamAppId == appId);
			if (!exitsts)
			{
				try
				{
					SteamGameDto steamDto = await _dataGainService.GetSteamRuData(appId);
					if (steamDto == null)
					{
						steamDto = await _dataGainService.GetSteamEnData(appId);
					}
					if (steamDto != null)
					{
						SpyGameDto? spyDto = await _dataGainService.GetSpyData(appId);

						Dictionary<string, int> tags = _dataGainService.GetTags(spyDto);

						long owners = _dataGainService.GetOwners(spyDto);

						(double userScore, int reviewAmount) = await _dataGainService.GetUserScore(appId);

						PriceData price = await _dataGainService.GetGamePrice(steamDto.SteamAppId);

						int? initialPrice = price.InitialPrice ?? -1;

						int? finalPrice = price.FinalPrice ?? -1;

						string? currency = price.Currency;

						double vectorLength = 0;


						Game game = await GameCreate(
						appId, steamDto.Name, steamDto.ImgUrl, steamDto.DetailedDescription,
						userScore, reviewAmount, owners,
						initialPrice, finalPrice, currency, vectorLength, tags);

						await _context.Games.AddAsync(game);
						await _context.SaveChangesAsync();
						return true;
					}
					else return false;
				}
				catch
				{
					Console.WriteLine("Не удалось импортировать игру");
					return false;
				}
			}
			else return false;
		}

	 

		public async Task<GameResponse?> GetGame(int appId)
		{
			Game? game = await _context.Games
			.Include(gt => gt.GameTags).ThenInclude(t => t.Tag)
			.FirstOrDefaultAsync(g => g.SteamAppId == appId);

			if (game == null)
			{
				return null;
			}

			var details = new GameResponse
			{
				Name = game.Name,

				Tags = game.GameTags.Select(gt => new SpyTag
				{
					Name = gt.Tag.Name,
					Weight = gt.Weight
				}).ToList(),

				DetailedDescription = game.DetailedDescription,
				Rating = game.Rating,
				Owners = game.Owners,
				InitialPrice = game.InitialPrice
			};
			return details;
		}

		//							"123 (ds3)": {rpg:0.5, sols-like:0.7}
		public async Task<Dictionary<string, Dictionary<string, double>>> GetGameTagsStrengh(int gameId)
		{
			bool exists = await _context.Games.AnyAsync(g => g.SteamAppId == gameId);


			if (!exists)
			{
				await ImportGameAsync(gameId);
			}

			GameResponse game = await GetGame(gameId);

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



		public async Task UpdateAllGamePrice()
		{
			//				.Where(g => g.FinalPrice == -1 || g.FinalPrice == null || g.FinalPrice == 0)
			List<int> appIds = await _context.Games

				.OrderBy(i => i.Id)
				.Select(x => x.Id)
				.ToListAsync();

			int count = 0;
			foreach (var appId in appIds)
			{
				count++;
				await UpdateGamePrice(appId);
				if (count == 10)
				{
					count = 0;
					await _context.SaveChangesAsync();
				}
			}
		}

		public async Task UpdateGamePrice(int appId)
		{
			Game dbGame = await _context.Games.FirstOrDefaultAsync(g => g.Id == appId);

			var freshData = new PriceData();

			try
			{
				freshData = await _dataGainService.GetGamePrice(dbGame.SteamAppId);
			}

			catch
			{
				Console.WriteLine("Не удалось получить цену");
			}

			dbGame.UpdatePriceData(freshData);
		}


		public async Task UpdateAllImgAsync()
		{
			List<int> appIds = await _context.Games.Select(x => x.Id).ToListAsync();

			foreach (var appId in appIds)
			{
				await UpdateImgAsync(appId);
			}
		}


		public async Task UpdateImgAsync(int appId)
		{
			Game dbGame = await _context.Games.FirstOrDefaultAsync(g => g.Id == appId);
			if (string.IsNullOrEmpty(dbGame.ImgUrl))
			{
				SteamGameDto freshData = await _dataGainService.GetSteamRuData(dbGame.SteamAppId);
				if (freshData != null)
				{
					dbGame.UpdateImageUrl(freshData.ImgUrl);
				}
				await _context.SaveChangesAsync();
			}
		}

		public async Task AddGameVector()
		{
			var gameTags = await _context.GameTags.ToListAsync();
			var games = await _context.Games.ToDictionaryAsync(x => x.Id);

			foreach (var tags in gameTags.GroupBy(x => x.GameId))
			{
				int sum = tags.Sum(x => x.Weight);

				if (sum == 0)
					continue;

				double sqrSum = 0;

				foreach (var tag in tags)
				{
					double strength = (double)tag.Weight / sum;
					tag.UpdateStrength(strength);
					sqrSum += strength * strength;
				}
				var length = Math.Sqrt(sqrSum);
				games[tags.Key].UpdateVectorLength(length);
			}

			await _context.SaveChangesAsync();
		}
	}
}
