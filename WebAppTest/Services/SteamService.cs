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
				InitialPrice = game.InitialPrice ?? 0
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
		public async Task<bool> ImportGameAsync(int appId)
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

					//int? initialPrice = await _dataGainService.GetInitPrice(appId);

					double vectorLength = 0;

					List<string> genres = await _dataGainService.GetGenres(appId);
					

					Game game = await _createService.GameCreate(
					appId, steamDto.Name, steamDto.ImgUrl, steamDto.DetailedDescription,
					userScore, reviewAmount, owners,
					steamDto.Price.Initial, steamDto.Price.Final, vectorLength, genres, tags);

					await _createService.AddGame(game);
					return true;
				}
				else return false;
			}
			else return false;
		}
		/////////////
		
	}
}
