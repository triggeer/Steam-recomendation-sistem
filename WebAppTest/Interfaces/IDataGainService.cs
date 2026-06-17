using WebAppTest.DTOs;

namespace WebAppTest.Interfaces
{
	public interface IDataGainService
	{
		Task<bool> CheckGameExistense(int appId);
		Task<bool> ChekUserTagVectorExistense(string userId);
		Task<SpyGameDto> GetSpyData(int appId);
		Task<int> GetInitPrice(int appId);
		Task<Dictionary<string, int>> GetTags(int appId);
		Task<List<string>> GetGenres(int appId);
		Task<SteamGameDto> GetSteamData(int appId);
		//Task<List<OwnedGameDto>> GetUserGamesIdList(string userId);
		//Task<List<UserGameDto>> GetUserGames(string userId);
	}
}
