using WebAppTest.DTOs;

namespace WebAppTest.Interfaces
{
	public interface IUserService
	{
		Task<UserVectorResponse?> GetUserVector(string userId);
		Task<List<OwnedGameDto>> GetUserGamesIdList(string userId);
		Task<List<UserGameDto>> GetUserGames(string userId);
		//Task<Dictionary<string, int>> GetUserGameTags(string userId);
		Task<Dictionary<string, Dictionary<string, double>>> GetUserGameTags(string userId);
		//Task<Dictionary<string, Dictionary<string, double>>> FormUserTagVector(string userId);
		Task<Dictionary<string, Dictionary<string, double>>> FormGameTagVector(int gameId);
		Task<Dictionary<string, Dictionary<string, double>>> GetGameTagsStrengh(int gameId);
		Task AddUserTagVector(string userId);
		Task UpdateUserVector(string userIdq);


	}
}
