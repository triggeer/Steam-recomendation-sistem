using WebAppTest.DTOs;

namespace WebAppTest.Interfaces
{
	public interface IDataGainService
	{
		Task<bool> CheckGameExistense(int appId);
		Task<bool> ChekUserTagVectorExistense(string userId);
		Task<SpyGameDto> GetSpyData(int appId);
		Task<(double, int)> GetUserScore(int appId);
		Task<long> GetOwners(int appId);
		long GetOwners(SpyGameDto spyDto);
		Task<int?> GetInitPrice(int appId);
		Task<Dictionary<string, int>> GetTags(int appId);
		Dictionary<string, int> GetTags(SpyGameDto spyDto);
		Task<List<string>> GetGenres(int appId);
		List<string> GetGenres(SteamGameDto steamDto);
		Task<SteamGameDto> GetSteamData(int appId);
		Task<int> GetCurrentGameAmount(string userId);
		//Task<List<OwnedGameDto>> GetUserGamesFromSteamAsync(string userId);
		//Task<List<UserGameDto>> GetUserGames(string userId);
	}
}
