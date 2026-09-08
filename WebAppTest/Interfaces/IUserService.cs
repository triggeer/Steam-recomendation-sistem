using WebAppTest.DTOs;
using WebAppTest.Models;

namespace WebAppTest.Interfaces
{
	public interface IUserService
	{
		Task<UserVectorResponse?> GetUserVector(string userId);
		Task<List<OwnedGameDto>> GetUserGamesFromSteamAsync(string userId);
		Task<List<UserGameDto>> GetUserGames(string userId, List<OwnedGameDto> ownedGames);
		Task<Dictionary<int, Dictionary<string, double>>> GetUserGameTags(string userId, List<OwnedGameDto> ownedGames);
		Task<Dictionary<string, Dictionary<string, double>>> GetGameTagsStrengh(int gameId);
		Task<Dictionary<string, double>> GetGameTagsStrengh1(int gameId);
		Task<UserProfile> CreateUserProfile(string userId);
		Task AddUserTagVector(string userId);
		Task UpdateUserProfile(string userIdq);
		Task UpdateUserProfile(UserProfile? oldProfile);
		Task<string> TransformLinkToId(string userLink);
		Task<List<OwnedGameDto>?> GetActualUserGames(string userId);

	}
}

