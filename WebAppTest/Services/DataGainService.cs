using System;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
		}

		public async Task<bool> ChekUserTagVectorExistense(string userId)
		{
			bool exists = await _context.UserProfiles.AnyAsync(u => u.Id == userId);
			return exists;
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

		public async Task<int?> GetInitPrice(int appId)
		{
			var spyDto = await GetSpyData(appId);
			if (spyDto.InitialPrice == null)
			{
				spyDto.InitialPrice = 0;
			}
			int? initialPrice = spyDto.InitialPrice;
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

		public async Task<(double, int)> GetUserScore(int appId)
		{
			string url = $"https://store.steampowered.com/appreviews/{appId}?json=1&language=all";
			
			HttpResponseMessage response = await _httpClient.GetAsync(url);

			response.EnsureSuccessStatusCode();

			string spyJsonString = await response.Content.ReadAsStringAsync();

			var responseData = JsonSerializer.Deserialize<SteamReviewResponse>(spyJsonString)
			?? throw new Exception("SteamSpy DTO is null");

			int total = responseData.data.Total;
			int positive = responseData.data.Positive;
			double score = 0;
			if (total != 0)
			{
				score = (double)positive / total;
			}
			double result = Math.Round(score, 2);
			return (result, total);
		}

		public async Task<long> GetOwners(int  appId)
		{
			var spyDto = await GetSpyData(appId);
			string ownersString = spyDto.Owners;
			string firstPart = ownersString.Split("..")[0].Trim();
			long owners = long.Parse(firstPart, NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
			return owners;
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

		public async Task<int> GetCurrentGameAmount(string userId)
		{
			var apiKey = _configuration.GetValue<string>("Steam:ApiKey");
			string url = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={apiKey}&steamid={userId}&format=json";
			var data = await _httpClient.GetFromJsonAsync<Dictionary<string, UserGamesResponse>>(url);
			int gameAmount = data["response"].game_count;
			return gameAmount;
		}
	}
}
