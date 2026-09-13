using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.DTOs;
using WebAppTest.Interfaces;
using WebAppTest.Models;

namespace WebAppTest.Services.User
{
	public class UserListService : IUserListService
	{
		private readonly HttpClient _httpClient;
		private readonly AppDbContext _context;

		public UserListService(
		HttpClient httpClient,
		AppDbContext context
		) 
		{ 
			_context = context;
		}


		/// <summary>
		/// Записывает данные сформированного списка рекомендаций в БД
		/// </summary>
		/// <param name="recommendedList">Сформированный список рекомендаций</param>
		/// <returns>Записанные данные о сформированном списке в БД</returns>
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

		public async Task<List<PrevListDto>> CollectFormedList(string userId)
		{
			List<PrevListDto> listOfLists = new();

			List<int> userListsIds = await _context.UserLists
				.Where(u => u.UserId == userId)
				.Select(l => l.ListId)
				.ToListAsync();

			foreach (var listId in userListsIds)
			{
				List<ListGame> listGames = await _context.ListGames
					.Where(x => x.ListId == listId)
					.OrderBy(g => g.GamePosition)
					.ToListAsync();

				List<int> idList = listGames.Select(x => x.GameId).ToList();


				List<GamePreviewDto> gameDetails = await _context.Games
					.Where(x => idList.Contains(x.Id))
					.Select(g => new GamePreviewDto
					{
						Id = g.Id,
						Name = g.Name,
						ImgUrl = g.ImgUrl
					})
					.ToListAsync();

				Dictionary<int, int> positions = listGames.ToDictionary(x => x.GameId, x => x.GamePosition);

				gameDetails = gameDetails.OrderBy(g => positions[g.Id]).ToList();

				var prevList = new PrevListDto()
				{
					Id = listId,
					Games = gameDetails
				};

				listOfLists.Add(prevList);
			}

			return listOfLists;
		}


		/// <summary>
		/// Формирует HashSet с id игр, которые уже были в сформированных списках рекомендаций
		/// </summary>
		/// <returns>хеш-таблица для быстрого поиска id, которые будем избегать</returns>
		public HashSet<int> FormIdBanList(string userId)
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

		

		
	}
}
