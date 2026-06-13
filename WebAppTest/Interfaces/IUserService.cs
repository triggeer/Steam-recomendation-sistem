using WebAppTest.DTOs;

namespace WebAppTest.Interfaces
{
	public interface IUserService
	{
		Task<List<OwnedGameDto>> GetUserGamesIdList(string userId);
		Task<List<UserGameDto>> GetUserGames(string userId);
		//Task<Dictionary<string, int>> GetUserGameTags(string userId);
		Task<Dictionary<string, Dictionary<string, double>>> GetUserGameTags(string userId);
	}
}
