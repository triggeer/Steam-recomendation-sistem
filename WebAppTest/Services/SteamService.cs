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
		private readonly IDBService _dbservice;

		public SteamService(
			HttpClient httpClient,
			IConfiguration configuration,
			AppDbContext context,
			IDataGainService dataGainService,
			ICreateService createService,
			IDBService dbservice)
		{
			_httpClient = httpClient;
			_configuration = configuration;
			_context = context;
			_dataGainService = dataGainService;
			_createService = createService;
			_dbservice = dbservice;
		}

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

		public async Task<bool> ImportGameAsync(int appId)
		{
			bool exitsts = await _dataGainService.CheckGameExistense(appId);
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
						SpyGameDto spyDto = await _dataGainService.GetSpyData(appId);

						Dictionary<string, int> tags = _dataGainService.GetTags(spyDto);

						(double userScore, int reviewAmount) = await _dataGainService.GetUserScore(appId);

						long owners = _dataGainService.GetOwners(spyDto);

						PriceData price = await _dbservice.GetGamePrice(steamDto.SteamAppId);

						int? initialPrice = price.InitialPrice ?? -1;

						int? finalPrice = price.FinalPrice ?? -1;

						string? currency = price.Currency;

						double vectorLength = 0;

						List<string> genres = _dataGainService.GetGenres(steamDto);

						Game game = await _createService.GameCreate(
						appId, steamDto.Name, steamDto.ImgUrl, steamDto.DetailedDescription,
						userScore, reviewAmount, owners,
						initialPrice, finalPrice, currency, vectorLength, genres, tags);

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
		
	}
}
