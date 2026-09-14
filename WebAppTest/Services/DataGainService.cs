using System;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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

		public async Task<PriceData> GetGamePrice(int steamId)
		{
			var priceData = new PriceData();
			SteamGameDto steamData = await GetSteamRuData(steamId);
			if (steamData != null && steamData.Price != null)
			{
				priceData.InitialPrice = steamData.Price.Initial;
				priceData.FinalPrice = steamData.Price.Final;
				priceData.Currency = "RUB";
				return priceData;
			}

			steamData = await GetSteamEnData(steamId);
			if (steamData != null && steamData.Price != null)
			{
				priceData.InitialPrice = steamData.Price.Initial;
				priceData.FinalPrice = steamData.Price.Final;
				priceData.Currency = "USD";
				return priceData;
			}

			SpyGameDto spyData = await GetSpyData(steamId);
			if (spyData != null)
			{
				priceData.InitialPrice = spyData.InitialPrice;
				priceData.FinalPrice = spyData.FinalPrice;
				priceData.Currency = "USD";
				return priceData;
			}

			return new PriceData { FinalPrice = -1 };
		}

		public async Task<Dictionary<string, int>> GetTags(int appId)
		{
			SpyGameDto spyDto = await GetSpyData(appId);
			return GetTags(spyDto);
		}

		public Dictionary<string, int> GetTags(SpyGameDto spyDto)
		{
			var tags = new Dictionary<string, int>();

			if (spyDto.Tags?.ValueKind == JsonValueKind.Object)
			{
				foreach (JsonProperty property in spyDto.Tags?.EnumerateObject())
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

		public async Task<long> GetOwners(int appId)
		{
			SpyGameDto spyDto = await GetSpyData(appId);
			long owners = GetOwners(spyDto);
			return owners;
		}

		public long GetOwners(SpyGameDto spyDto)
		{
			string ownersString = spyDto.Owners;
			string firstPart = ownersString.Split("..")[0].Trim();
			long owners = long.Parse(firstPart, NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
			return owners;
		}



		public async Task<SpyGameDto?> GetSpyData(int appId)
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
			);

			return spyDto;
		}


		public async Task<SteamGameDto> GetSteamData(int appId, string steamUrl)
		{
			var steamResponse = await _httpClient.GetAsync(steamUrl);

			steamResponse.EnsureSuccessStatusCode();

			var steamJson = await steamResponse.Content.ReadAsStringAsync();

			var steamData = JsonSerializer.Deserialize<
			Dictionary<string, SteamStoreResponse>
			>(steamJson);

			if (steamData[appId.ToString()].success == false)
			{
				steamUrl = $"https://store.steampowered.com/api/appdetails?appids={appId}";
				steamResponse = await _httpClient.GetAsync(steamUrl);
				steamResponse.EnsureSuccessStatusCode();
				steamJson = await steamResponse.Content.ReadAsStringAsync();
				steamData = JsonSerializer.Deserialize<
					Dictionary<string, SteamStoreResponse>
					>(steamJson);
			}

			// {"1938090":{"success":false}}
			SteamGameDto? steamDto = steamData?[appId.ToString()]?.data;

			return steamDto;
		}

		public async Task<SteamGameDto> GetSteamEnData(int appId)
		{
			var steamGameData = new SteamGameDto();
			string steamUrl = $"https://store.steampowered.com/api/appdetails?appids={appId}";
			steamGameData = await GetSteamData(appId, steamUrl);
			return steamGameData;
		}

		public async Task<SteamGameDto> GetSteamRuData(int appId)
		{
			var steamGameData = new SteamGameDto();
			string steamUrl = $"https://store.steampowered.com/api/appdetails?appids={appId}&cc=ru&l=russian";
			steamGameData = await GetSteamData(appId, steamUrl);
			return steamGameData;
		}






		public async Task<int> GetCurrentGameAmount(string userId)
		{
			var apiKey = _configuration.GetValue<string>("Steam:ApiKey");
			string url = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={apiKey}&steamid={userId}&format=json";
			var data = await _httpClient.GetFromJsonAsync<Dictionary<string, UserGamesResponse>>(url);
			int gameAmount = data["response"].gameСount;
			return gameAmount;
		}

		public async Task<List<OwnedGameDto>> GetUserGamesFromSteamAsync(string userId)
		{
			var apiKey = _configuration.GetValue<string>("Steam:ApiKey");
			var url = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={apiKey}&steamid={userId}&format=json";

			var userGameList = new List<Game>();

			var steamResponse = await _httpClient.GetAsync(url);
			steamResponse.EnsureSuccessStatusCode();

			var steamJson = await steamResponse.Content.ReadAsStringAsync();
			var steamData = JsonSerializer.Deserialize<
			Dictionary<string, UserGamesResponse>
			>(steamJson);

			List<OwnedGameDto> ownedGames = steamData["response"].games;

			return ownedGames; // (id, часы)
		}





		public async Task<List<int>> GetNewSpyGameIds()
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
				Dictionary<string, SpyGameDto>
			>(json, options);

			List<int> ids = [];

			foreach (var game in data.Values)
			{
				ids.Add(game.AppId);
			}

			return ids;
		}




		public async Task<string> TransformLinkToId(string userLink)
		{
			string steamLink = $"{userLink}?xml=1";

			Stream steamResponse = await _httpClient.GetStreamAsync(steamLink);

			XDocument xdoc = XDocument.Load(steamResponse);

			XElement? root = xdoc.Root;

			Uri uri = new Uri(userLink);

			string userId = root.Element("steamID64").Value;

			return userId;
		}
	}
}
