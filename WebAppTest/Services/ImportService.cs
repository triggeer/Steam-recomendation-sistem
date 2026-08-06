
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
		private readonly AppDbContext _context;
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private readonly ISteamService _steamService;
		private readonly IDataGainService _dataGainService;

		public ImportService(
			HttpClient httpClient,
			IConfiguration configuration,
			AppDbContext context,
			ISteamService steamService)
		{
			_httpClient = httpClient;
			_configuration = configuration;
			_context = context;
			_steamService = steamService;
		}


		public async Task<List<int>> Get100Games()
		{
			/* ПОЛУЧАЕМ 100 ИГРЫ С ID ИЗ JSON
			 * ЗАНОСИМ ID В СПИСОК
			 * ДЛЯ КАЖДОГО ID ИЗ СПИСКА ПОЛУЧАЕМ ИНФУ ОБ ИГРЕ 
			 */
			//string url = "https://steamspy.com/api.php?request=top100in2weeks";
			//string url = "https://steamspy.com/api.php?request=all&page=0";
			string url = "https://steamspy.com/api.php?request=all&page=1";
			HttpResponseMessage response = await _httpClient.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string json = await response.Content.ReadAsStringAsync();

			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};

			var data = JsonSerializer.Deserialize<
				Dictionary<string, Syp100Dto>
			>(json, options);

			List<int> ids = [];
			
			foreach (var game in data.Values)
			{
				ids.Add(game.AppId);
			}
			
			return ids;
		}

		public async Task Import100Games(List<int> ids)
		{
			List<int> idToShow= new List<int>();
			foreach (var id in ids)
			{
				idToShow.Add(id);
				await _steamService.ImportGameAsync(id);

			}
		}
	}
}
