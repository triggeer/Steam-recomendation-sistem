using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;

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


		//public async Task UpdateGamePrice(int appId)
		//{
		//	var exists = await _dataGainService.CheckGameExistense(appId);
		//	if (!exists)
		//	{
		//		await _steamService.ImportGameAsync(appId);
		//		return;
		//	}
		//	else
		//	{
		//		var oldGame = await _context.Games
		//							.FirstOrDefaultAsync(g => g.SteamAppId == appId);


		//		int? newInitialPrice = await _dataGainService.GetInitPrice(appId);


		//		if (oldGame.InitialPrice == newInitialPrice)
		//			return;
		//		else
		//		{
		//			oldGame.UpdateInitialPrice(newInitialPrice);
		//			await _context.SaveChangesAsync();
		//			return;
		//		}
		//	}
		//}

		public async Task UpdateImgAsync()
		{
			List<int> appIds = await _context.Games.Select(x => x.Id).ToListAsync();

			foreach (var appId in appIds)
			{
				var dbGame = await _context.Games.FirstOrDefaultAsync(g => g.Id == appId);
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
}
