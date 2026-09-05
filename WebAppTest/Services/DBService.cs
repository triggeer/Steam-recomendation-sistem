using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Migrations;
using WebAppTest.Models;

namespace WebAppTest.Services
{
	public class DBService : IDBService
	{
		private readonly AppDbContext _context;
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private readonly IDataGainService _dataGainService;

		public DBService(
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
				freshData = await GetGamePrice(dbGame.SteamAppId);
			}

			catch
			{
				Console.WriteLine("Не удалось получить цену");
			}

			dbGame.UpdatePriceData(freshData);
		}

		public async Task<PriceData> GetGamePrice(int steamId)
		{
			var priceData = new PriceData();
			SteamGameDto steamData = await _dataGainService.GetSteamRuData(steamId);
			if (steamData != null && steamData.Price != null)
			{
				priceData.InitialPrice = steamData.Price.Initial;
				priceData.FinalPrice = steamData.Price.Final;
				priceData.Currency = "RUB";
				return priceData;
			}
			
			steamData = await _dataGainService.GetSteamEnData(steamId);
			if (steamData != null && steamData.Price != null)
			{
				priceData.InitialPrice = steamData.Price.Initial;
				priceData.FinalPrice = steamData.Price.Final;
				priceData.Currency = "USD";
				return priceData;
			}

			SpyGameDto spyData = await _dataGainService.GetSpyData(steamId);
			if (spyData != null)
			{
				priceData.InitialPrice = spyData.InitialPrice;
				priceData.FinalPrice = spyData.FinalPrice;
				priceData.Currency = "USD";
				return priceData;
			}

			return new PriceData { FinalPrice = -1 };
			
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
		

	}
}
