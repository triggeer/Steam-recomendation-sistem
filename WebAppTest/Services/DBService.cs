using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Models;

namespace WebAppTest.Services
{
	public class DBService : IDBService
	{
		private readonly AppDbContext _context;
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private readonly IDataGainService _dataGainService;
		//private readonly ICreateService _createService;
		//private readonly ISteamService _steamService;

		public DBService(
			HttpClient httpClient,
			IConfiguration configuration,
			AppDbContext context,
			IDataGainService dataGainService,
			ISteamService steamService,
			ICreateService createService)
		{
			_httpClient = httpClient;
			_configuration = configuration;
			_context = context;
			_dataGainService = dataGainService;
			//_steamService = steamService;
			//_createService = createService;
		}


		public async Task UpdateAllGamePrice()
		{
			List<int> appIds = await _context.Games.Select(x => x.Id).ToListAsync();
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

			SteamGameDto freshData = await _dataGainService.GetSteamData(dbGame.SteamAppId);
			if (freshData != null)
			{
				if (freshData.Price == null)
					dbGame.UpdateFinalPrice(0);
				else
					dbGame.UpdateFinalPrice(freshData.Price.Final);
			}
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
				SteamGameDto freshData = await _dataGainService.GetSteamData(dbGame.SteamAppId);
				if (freshData != null)
				{
					dbGame.UpdateImageUrl(freshData.ImgUrl);
				}
				await _context.SaveChangesAsync();
			}
		}

	}
}
