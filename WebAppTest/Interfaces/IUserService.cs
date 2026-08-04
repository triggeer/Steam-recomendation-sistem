using WebAppTest.DTOs;

namespace WebAppTest.Interfaces
{
	public interface IUserService
	{
		Task<UserVectorResponse?> GetUserVector(string userId);
		Task<List<OwnedGameDto>> GetUserGamesIdList(string userId);
		Task<List<UserGameDto>> GetUserGames(string userId);
		Task<Dictionary<int, Dictionary<string, double>>> GetUserGameTags(string userId);
		Task<Dictionary<string, Dictionary<string, double>>> GetGameTagsStrengh(int gameId);
		Task<Dictionary<string, double>> GetGameTagsStrengh1(int gameId);
		Task AddUserTagVector(string userId);
		Task UpdateUserVector(string userIdq);


	}
}
