
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;

namespace WebAppTest.Services
{
	public class ImportService : IImportService
	{
		private readonly HttpClient _httpClient;
		private readonly IGameService _gameService;
		private readonly IDataGainService _dataGainService;

		public ImportService(
			HttpClient httpClient,
			IGameService gameService,
			IDataGainService dataGainService
			)
		{
			_gameService = gameService;
			_dataGainService = dataGainService;
		}


		public async Task ImportNewGames()
		{
			List<int> ids = await _dataGainService.GetNewSpyGameIds();

			foreach (var id in ids)
				await _gameService.ImportGameAsync(id);
			
		}

		public async Task ImportNewGames(List<int> ids)
		{
			foreach (var id in ids)
				await _gameService.ImportGameAsync(id);
		}

		//public async Task ImportNewGames(List<int> ids)
		//{
		//	List<int> idToShow = new List<int>();
		//	foreach (var id in ids)
		//	{
		//		idToShow.Add(id);
		//		await _gameService.ImportGameAsync(id);
		//	}
		//}
	}
}
