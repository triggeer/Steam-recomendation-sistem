
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
		private readonly ISteamService _steamService;

		public ImportService(
			HttpClient httpClient,
			ISteamService steamService)
		{
			_httpClient = httpClient;
			_steamService = steamService;
		}


		public async Task<List<int>> GetNewGames()
		{
			string url = "https://steamspy.com/api.php?request=all&page=1";
			HttpResponseMessage response = await _httpClient.GetAsync(url);
			
			response.EnsureSuccessStatusCode();

			string json = await response.Content.ReadAsStringAsync();

			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};

			var data = JsonSerializer.Deserialize<
				Dictionary<string, Spy100Dto>
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
