using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql.PostgresTypes;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Models;

namespace WebAppTest.Services
{
	public class DataGainService : IDataGainService
	{
		private readonly AppDbContext _context;
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;


		public DataGainService(
			HttpClient httpClient,
			IConfiguration configuration,
			AppDbContext context)
		{
			_httpClient = httpClient;
			_configuration = configuration;
			_context = context;

		}

		public async Task<bool> CheckGameExistense(int appId)
		{
			bool exists = await _context.Games.AnyAsync(g => g.SteamAppId == appId);
			return exists;	
			//if (exists)
			//{
			//	return true;
			//}
			//else
			//{
			//	return false;
			//}
		}

		public async Task<SpyGameDto> GetSpyData(int appId)
		{
			string spyUrl = $"https://steamspy.com/api.php?request=appdetails&appid={appId}";
			
			HttpResponseMessage spyResponse = await _httpClient.GetAsync(spyUrl);
			
			spyResponse.EnsureSuccessStatusCode();

			string spyJsonString = await spyResponse.Content.ReadAsStringAsync();

			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};

			SpyGameDto? spyDto = JsonSerializer.Deserialize<SpyGameDto>(
				spyJsonString,
				options
			) ?? throw new Exception("SteamSpy DTO is null");
			
			return spyDto;
		}

		public async Task<int> GetInitPrice(int appId)
		{
			var spyDto = await GetSpyData(appId);
			if (spyDto.InitialPrice == null)
			{
				spyDto.InitialPrice = "0";
			}
			int initialPrice = int.Parse(spyDto.InitialPrice);
			return initialPrice;
		}

		public async Task<Dictionary<string, int>> GetTags(int appId)
		{
			var tags = new Dictionary<string, int>();
			SpyGameDto spyDto = await GetSpyData(appId);

			if (spyDto.Tags.ValueKind == JsonValueKind.Object)
			{
				foreach (JsonProperty property in spyDto.Tags.EnumerateObject())
				{
					tags[property.Name] = property.Value.GetInt32();
				}
			}
			
			return tags;
		}

		public async Task<List<string>> GetGenres(int appId)
		{
			var genres = new List<string>();
			var steamDto = await GetSteamData(appId);
			if (steamDto.Genres != null)
			{
				genres = steamDto.Genres.Select(g => g.Description).ToList(); //////////////////
				return genres;
			}
			else return genres = [];
		}

		public async Task<SteamGameDto> GetSteamData(int appId)
		{
			var steamUrl = $"https://store.steampowered.com/api/appdetails?appids={appId}&l=russian";

			var steamResponse = await _httpClient.GetAsync(steamUrl);

			steamResponse.EnsureSuccessStatusCode();

			var steamJson = await steamResponse.Content.ReadAsStringAsync();

			var steamData = JsonSerializer.Deserialize<
			Dictionary<string, SteamStoreResponse>
			>(steamJson);

			SteamGameDto? steamDto = steamData?[appId.ToString()]?.data;

			return steamDto;
		}
	}
}
