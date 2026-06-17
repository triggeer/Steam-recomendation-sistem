using System.Numerics.Tensors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;

namespace WebAppTest.Services
{
	public class RecommendationService : IRecommendationService
	{
		private readonly HttpClient _httpClient;
		private readonly IUserService _userService;
		private readonly IDataGainService _dataGainService;
		private readonly ICreateService _createService;
		
		public RecommendationService(
		HttpClient httpClient,
		IUserService userService,
		IDataGainService dataGainService,
		ICreateService createService
		)
		{
			_httpClient = httpClient;
			_userService = userService;
			_dataGainService = dataGainService;
			_createService = createService;
		}

		public async Task<double> CosSimilarity(
			Dictionary<string, double> userTags,
			Dictionary<string, double> gameTags)
		{
			double counter = 0;

			//// userVector = {"123123 (bogdan)": {"rpg": 0.2, "govno": 0.7}}
			//// userTags = {"rpg": 0.2, "govno": 0.7}
			//// gameTags = {"rpg": 0.3, "govno": 0.6}
			foreach (var tag in gameTags)
			{
				if (userTags.TryGetValue(tag.Key, out double userWeight))
				{
					// считаем сколярное произведение - то на сколько сильно совпадают теги пользователя и игры
					/* если 0.1 * 0.1 = 0.01
					 * если 0.01 * 0.01 = 0.0001
					 * по итогу будет типа 0.0357 если много совпадений
					 * если ничё не совпадает будет 0.0043 */
					counter += tag.Value * userWeight;
				}
			}

			// ищем длины векторов 
			/*
			 * 0.10² = 0.0100
				0.08² = 0.0064
				0.12² = 0.0144
				0.05² = 0.0025
				0.02² = 0.0004
				= 0.0337
				√0.0337 = 0.1836
			 */
			double userNorm =
				Math.Sqrt(userTags.Values.Sum(v => v * v));

			double gameNorm =
				Math.Sqrt(gameTags.Values.Sum(v => v * v));

			if (userNorm == 0 || gameNorm == 0)
				return 0;

			// возвращаем Косинусное сходство векторов (0.3)
			return counter / (userNorm * gameNorm);
		}

		//public async Task GetUserVector()

		public async Task<UserVectorResponse> FormUserTagVector(string userId)
		{
			// проверяем, есть ли уже сформированный вектор
			bool exists = await _dataGainService.ChekUserTagVectorExistense(userId);
			
			// если нет - добавляем
			if (!exists)
			{
				await _userService.AddUserTagVector(userId);
			}

			// берем инфу из бд
			var vector = await _userService.GetUserVector(userId);
			return vector; // гуд
		}
	}
}
