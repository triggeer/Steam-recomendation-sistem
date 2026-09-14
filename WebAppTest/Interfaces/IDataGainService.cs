using WebAppTest.DTOs;

namespace WebAppTest.Interfaces
{
	public interface IDataGainService
	{
		Task<SpyGameDto> GetSpyData(int appId);
		Task<(double, int)> GetUserScore(int appId);
		Task<long> GetOwners(int appId);
		long GetOwners(SpyGameDto spyDto);
		Task<PriceData> GetGamePrice(int steamId);
		Task<Dictionary<string, int>> GetTags(int appId);
		Dictionary<string, int> GetTags(SpyGameDto spyDto);
		Task<SteamGameDto> GetSteamEnData(int appId);
		Task<SteamGameDto> GetSteamRuData(int appId);
		Task<int> GetCurrentGameAmount(string userId);
		Task<List<OwnedGameDto>> GetUserGamesFromSteamAsync(string userId);
		Task<List<int>> GetNewSpyGameIds();
		Task<string> TransformLinkToId(string userLink);
	}
}
