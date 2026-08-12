using System.Collections;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Components.Forms.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Validation;
using Npgsql.TypeMapping;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Migrations;
using WebAppTest.Models;
using WebAppTest.Services;

namespace WebAppTest.Services
{
	public class RecommendationService : IRecommendationService
	{
		private readonly AppDbContext _context;
		private readonly HttpClient _httpClient;
		private readonly IUserService _userService;
		private readonly IDataGainService _dataGainService;
		private readonly ICreateService _createService;
		
		public RecommendationService(
		AppDbContext context,
		HttpClient httpClient,
		IUserService userService,
		IDataGainService dataGainService,
		ICreateService createService
		)
		{
			_context = context;
			_httpClient = httpClient;
			_userService = userService;
			_dataGainService = dataGainService;
			_createService = createService;
		}

		public double CosSimilarity(
			Dictionary<int, double> firstGameTags, 
			Dictionary<int, double> secondGameTags,
			double fristVectorLen, double secondVectorLen
		)
		{
			double scalar = 0;

			//// userVector = {"123123 (bogdan)": {"rpg": 0.2, "govno": 0.7}}
			//// userTags = {"rpg": 0.2, "govno": 0.7}
			//// gameTags = {"rpg": 0.3, "govno": 0.6}
			foreach (var tag in secondGameTags)
			{
				if (firstGameTags.TryGetValue(tag.Key, out double firstWeight))
				{
					// считаем сколярное произведение - то на сколько сильно совпадают теги пользователя и игры
					/* если 0.1 * 0.1 = 0.01
					 * если 0.01 * 0.01 = 0.0001
					 * по итогу будет типа 0.0357 если много совпадений
					 * если ничё не совпадает будет 0.0043 */
					scalar += tag.Value * firstWeight;
				}
			}

			if (fristVectorLen == 0 || secondVectorLen == 0)
				return 0;

			// возвращаем Косинусное сходство векторов (0.3)
			return scalar / (fristVectorLen * secondVectorLen);
		}

		public async Task AddGameVector()
		{
			var gameTags = await _context.GameTags.ToListAsync();
			var games = await _context.Games.ToDictionaryAsync(x => x.Id);

			foreach (var tags in gameTags.GroupBy(x => x.GameId))
			{
				int sum = tags.Sum(x => x.Weight);

				if (sum == 0)
					continue;

				double sqrSum = 0;
				
				foreach (var tag in tags)
				{ 
					double strength = (double)tag.Weight / sum;
					tag.UpdateStrength(strength);
					sqrSum += strength * strength;
				}
				var length = Math.Sqrt(sqrSum);
				games[tags.Key].UpdateVectorLength(length);
			}

			await _context.SaveChangesAsync();
		}


		public async Task<UserVectorResponse> FormUserTagVector(string userId)
		{
			// проверяем, есть ли уже сформированный вектор
			bool exists = await _dataGainService.ChekUserTagVectorExistense(userId);
			
			// если нет - добавляем
			if (!exists)
			{
				await _userService.AddUserTagVector(userId);
			}

			int gameAmount = await _dataGainService.GetCurrentGameAmount(userId);
			DateTime currentDate = DateTime.Now;
			// берем инфу из бд
			UserVectorResponse vector = await _userService.GetUserVector(userId);

			
			if (vector.GameAmount != gameAmount || (currentDate - vector.UpdatedAt) > TimeSpan.FromDays(7))
			{

				await _userService.UpdateUserVector(userId);
			}


			return vector; // гуд
		}

		private async Task<Dictionary<int, double>> FormUserTagsDict(string userId)
		{
			UserVectorResponse userVector = await FormUserTagVector(userId);
			Dictionary<int, double> userTags = userVector.TagStrength;
			return userTags;
		}

		public async Task<List<RecommendationDto>> FormRecommendationListAsync(string userId)
		{
			List<RecommendationCandidate> recommendations = await FormUnsortedRecommendationList(userId);
			var list = new List<RecommendationDto>();
			foreach (var candidate in recommendations)
			{
				list.Add(candidate.Game);
			}
			// сортируем по рейтингу схожести
			List<RecommendationDto> sortedList = list.OrderByDescending(r => r.Score).ToList();

			return sortedList;
		}


		public async Task<RecommendationsOnTeg> FormRecomendationsOnTagAsync(string userId, int tagId)
		{
			var list = new List<RecommendationDto>();
			var recomendations = await FormRecommendationListAsync(userId);
			var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == tagId);
			foreach (var game in recomendations)
			{
				if (game.GameTags.Any(x => x.Key == tag.Id))
				{
					list.Add(game);
				}
			}

			var result = new RecommendationsOnTeg{ ListGame = list, tag = tag.Name};

			return result;
		}


		public async Task<List<RecommendationDto>> ForUniqueRecomendationsAsync(string userId)
		{
			var list11 = await CollectFormedList(userId);
			var banList = FormIdBanList(userId);
			var uniqueList = new List<RecommendationCandidate>();
			List<RecommendationCandidate> recGames = await FormUnsortedRecommendationList(userId);
			List<RecommendationCandidate> sortedCandidates = recGames.OrderByDescending(x => x.UserScore).ToList();
			sortedCandidates.RemoveAll(game => banList.Contains(game.Id));

			Dictionary<(int, int), double> similaritys = new();

			//var first = sortedCandidates.FirstOrDefault(x => !banList.Contains(x.Id));
			var first = sortedCandidates[0];
			first.FinalScore = first.UserScore;
			uniqueList.Add(first);
			sortedCandidates.Remove(first);

			// получаем словарь с id игры присвоенное стимом и  id игры в БД 

			var flatData = await _context.GameTags.ToListAsync();
			// словарь тегов формата "123 (ds3)": { 3 (rpg): 0.5, 5 (sols-like):0.7,...}
			Dictionary<int, Dictionary<int, double>> tags = flatData
				.GroupBy(x => x.GameId)
				.ToDictionary(
					x => x.Key, 
					x => x.ToDictionary(t => t.TagId, t => t.Strength)
				);

			Dictionary<int, double> vectorLengths = _context.Games.ToDictionary(x => x.Id, x => x.VectorLength);

			var chosenGame = new RecommendationCandidate();


			while (uniqueList.Count < 30)
			{
				/*
				 * MaxSimilarity не сбрасывается с каждым циклом, 
				 * а копится, чтобы оставлять уже вычисленное сходство кандидата с уже выбранными играми
				 */
				// для каждого кандидата из рекомендаций
				//(mhw, ggst, dota,...) НЕВЫБРАННЫЕ ИГРЫ
				foreach (var candidate in sortedCandidates)
				{

					// для всех оставшихся игр из кандидатов
					// сравниваем похожеcть тегов
					// каждый вектор ВЫБРАННОЙ игры
					// (valh, grounded) ВЫБРАННЫЕ
					chosenGame = uniqueList.Last();
					//foreach (var chosenGame in uniqueList)
					//{

						//double similarity = 0;
						
						int minId = Math.Min(chosenGame.Id, candidate.Id);
						int maxId = Math.Max(chosenGame.Id, candidate.Id);
						if (!similaritys.TryGetValue((minId, maxId), out double similarity))
						{
							Dictionary<int, double> chosenTags1 = tags[chosenGame.Id];
							Dictionary<int, double> candidateTags1 = tags[candidate.Id];

							double chosenVectorLen = vectorLengths[chosenGame.Id];
							double candidateVectorLen = vectorLengths[candidate.Id];

							similarity = CosSimilarity(chosenTags1, candidateTags1, chosenVectorLen, candidateVectorLen);
							similaritys.Add((minId, maxId), similarity);
						}

						candidate.MaxSimilarity = Math.Max(similarity, candidate.MaxSimilarity);


					//}
					//													если очень схоже с хоть одной из-
					//													-уже выбранных игр то биг штраф
					candidate.FinalScore = (0.8 * candidate.UserScore) - (0.2 * candidate.MaxSimilarity);

				}

				var winer = sortedCandidates.MaxBy(x => x.FinalScore);
				uniqueList.Add(winer);
				sortedCandidates.Remove(winer);

			}


			var list = new List<RecommendationDto>();
			foreach (var candidate in uniqueList)
			{
				candidate.Game.Score = candidate.FinalScore;
				list.Add(candidate.Game);
			}
			var sortedList = list.OrderByDescending(x => x.Score).ToList();

			await AddUserList(sortedList, userId);

			return sortedList;
		}


		private RecommendationCandidate CalculateRecomendationRating(
			Dictionary<int, double> userTags, 
			double userTagLength,
			Dictionary<int, Dictionary<int, double>> gameTagsInfo, 
			int gameId,
			string gameName,
			double gameVectorLen,
			int gameReviewAmount,
			double gameUserScore)
		{
			double finalScore = 0;
			if (gameTagsInfo.TryGetValue(gameId, out Dictionary<int, double> gameTags))
			{
				double similarity = CosSimilarity(userTags, gameTags, userTagLength, gameVectorLen);
				/*
					Monster Hunter: World: --> 0,5835905493661762 <---
					ELDEN RING NIGHTREIGN: --> 0,5826818525389187 <--- 
					Valheim: --> 0,5805596998270341 <---
				*/
				double rating = CalculateBayesRaiting(gameReviewAmount, gameUserScore);

				finalScore = similarity * rating;
			}

			RecommendationDto recommendationDto = new RecommendationDto
			{
				//GameId = game.SteamAppId,
				// меняем seam ID на id БД
				GameId = gameId,
				GameName = gameName,
				Score = finalScore,
				GameTags = gameTags
			};

			RecommendationCandidate candiadate = new RecommendationCandidate
			{
				Id = recommendationDto.GameId,
				Game = recommendationDto,
				UserScore = finalScore,
				MaxSimilarity = 0,
				FinalScore = 0
			};

			return candiadate;
		}

		private static double CalculateBayesRaiting(int reviewAmount, double score)
		{
			// минимальное количество голосов, необходимое для того, чтобы рейтинг стал значимым
			const int min = 50000;
			// средний рейтинг для всех объектов в базе
			const double avgScore = 0.5;

			double rating = ((reviewAmount * score) + (min * avgScore)) / (reviewAmount + min);
			return rating;
		}

		private async Task<List<RecommendationCandidate>> FormUnsortedRecommendationList(string userId)
		{
			var list = new List<RecommendationCandidate>();
			
			Dictionary<int, double> userTags = await FormUserTagsDict(userId);
			
			var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.Id == userId);
			
			double userTagLen = userProfile.Length;

			List<OwnedGameDto> userGames = await _userService.GetUserGamesIdList(userId);
			HashSet<int> userIds = userGames.Select(x => x.AppId).ToHashSet();

			var games = await _context.Games.Select(x => new { 
				x.Id, 
				x.Name, 
				x.VectorLength, 
				x.SteamAppId, 
				x.UserScore, 
				x.ReviewAmount}
				).ToListAsync();
			
			var tagInfo = await _context.GameTags.ToListAsync();

			Dictionary<int, Dictionary<int, double>> gameTagsInfo = tagInfo
				.GroupBy(x => x.GameId)
				.ToDictionary(
				x => x.Key,
				x => x.ToDictionary(t => t.TagId, t => t.Strength)
				);
			
			foreach (var game in games)
			{
				// если у пользователя нет игры с таким стим id делаем её рекомендацию со своим id
				if (!userIds.Contains(game.SteamAppId))
				{
					// меняем seam ID на id БД
					RecommendationCandidate recommendation = CalculateRecomendationRating(
						userTags, 
						userTagLen, 
						gameTagsInfo, 
						game.Id, game.Name,
						game.VectorLength, game.ReviewAmount,
						game.UserScore);

					if (recommendation.UserScore > 0)
					{
						list.Add(recommendation);
					}
				}
			}

			return list;
		}

		public async Task AddUserList(List<RecommendationDto> recommendedList, string userId)
		{
			DateTime updatedAt = DateTime.Now;
			RecList recList = new RecList(updatedAt);

			_context.RecLists.Add(recList);
			await _context.SaveChangesAsync();

			var gamesFromList = new List<ListGame>();

			int position = 0;
			foreach (var game in recommendedList)
			{
				ListGame listGame = new ListGame(recList.Id, game.GameId, position);
				position++;
				gamesFromList.Add(listGame);
			}

			_context.ListGames.AddRange(gamesFromList);

			var userList = new UserList(userId, recList.Id);

			_context.UserLists.Add(userList);

			await _context.SaveChangesAsync();

		}

		protected HashSet<int> FormIdBanList(string userId)
		{
			var emptyList = new HashSet<int>();

			var userListsData = _context.UserLists.Where(x => x.UserId == userId);
			if (userListsData.Any())
			{
				var userLists = userListsData
					.Select(x => x.ListId)
					.ToList();

				List<int> gameIds = _context.ListGames
					.Where(x => userLists.Contains(x.ListId))
					.Select(l => l.GameId)
					.ToList();
					
				HashSet<int> hashIds = gameIds.ToHashSet();
				return hashIds;
			}
			else return emptyList;
		}

		public async Task<Dictionary<int, List<int>>> CollectFormedList(string userId)
		{
			var listOfLists = new Dictionary<int, List<int>>();	

			//var previousList = new List<int>();

			List<int> userListsIds = await _context.UserLists
				.Where(u => u.UserId == userId)
				.Select(l => l.ListId)
				.ToListAsync();

			foreach (var listId in userListsIds)
			{
				List<int> gamesIds = await _context.ListGames
					.Where(x => x.ListId == listId)
					.OrderBy(g => g.GamePosition)
					.Select(i => i.GameId)
					.ToListAsync();

				listOfLists.Add(listId, gamesIds);
				//Dictionary<int, Game> gameData = await _context.Games
				//	.Where(g => gameDict.Contains(g.Id))
				//	.ToDictionary(g => g.Id, g => );

				
				//foreach (var gameId in gameDict)
				//{
				//	previousList.Add(gameData)
				//}
			}

			return listOfLists;
		}
	}
}
