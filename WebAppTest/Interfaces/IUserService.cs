using WebAppTest.DTOs;
using WebAppTest.Models;
using WebAppTest.Services.User;

namespace WebAppTest.Interfaces
{
	public interface IUserService
	{
		Task<UserVectorResponse?> GetUserVector(string userId);
		Task<List<UserGameData>> GetUserGames(string userId, List<OwnedGameDto> ownedGames);
		Task<Dictionary<int, Dictionary<string, double>>> GetUserGameTags(string userId, List<OwnedGameDto> ownedGames);
		Task<UserProfile> CreateUserProfile(string userId);
		Task AddUserTagVector(string userId);
		Task UpdateUserProfile(string userIdq);
		Task UpdateUserProfile(UserProfile? oldProfile);
		Task<List<OwnedGameDto>?> GetActualUserGames(string userId);
		Task<Dictionary<int, double>> GetUserTagVector(string userId);

	}
}

