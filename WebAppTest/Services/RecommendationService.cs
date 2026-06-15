using System.Numerics.Tensors;
using Microsoft.AspNetCore.Identity;
using WebAppTest.Interfaces;

namespace WebAppTest.Services
{
	public class RecommendationService : IRecommendationService
	{
		private readonly HttpClient _httpClient;
		private readonly IUserService _userService;
		private readonly ISteamService _steamService;
		private readonly IDataGainService _dataGainService;
		
		public RecommendationService(
		HttpClient httpClient,
		IUserService userService,
		ISteamService steamService,
		IDataGainService dataGainService
		)
		{
			_httpClient = httpClient;
			_userService = userService;
			_steamService = steamService;
			_dataGainService = dataGainService;
		}

		//public static float[] ReadArray(int elements, )

		public async Task<float> CosSimilarity(
		Dictionary<string, Dictionary<string, double>> userVector, 
		string userId,
		Dictionary<string, Dictionary<string, double>> gameVector,
		int gameId)
		{
			int userLengh = userVector[userId].Count;
			int gameLengh = gameVector[gameId.ToString()].Count;
			float[] floatUserVector = new float[userLengh];
			float[] floatGameVector = new float[gameLengh];
			
			for (int i = 0; i < floatUserVector.Length; i++)
			{
				foreach (var tag in userVector[userId])
				{
					floatUserVector[i] = (float)tag.Value;
					i++;
				}
			}

			for (int i = 0; i < floatGameVector.Length; i++)
			{
				foreach (var tag in gameVector[gameId.ToString()])
				{
					floatGameVector[i] = (float)tag.Value;
					i++;
				}
			}

			//// userVector = {"123123 (bogdan)": {"rpg": 0.2, "govno": 0.7}}
			ReadOnlySpan<float> user = floatUserVector;
			ReadOnlySpan<float> game = floatGameVector;
			// КОРОЧЕ ВСЁ ВЫШЕ НОРМ СЧИТАЕТ А ТУТА НАДО БУДЕТ СДЕЛАТЬ (ЩА БОБИК И ШЛЁПИК РАЗНЫХ ДЛИН, А НАДО ОДИНАКОВЫХ)
			var result = TensorPrimitives.CosineSimilarity(user, game);
			return result;

		}

	}
}
