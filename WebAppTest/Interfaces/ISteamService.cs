using WebAppTest.DTOs;
using WebAppTest.Models;

namespace WebAppTest.Interfaces
{
	public interface ISteamService
	{
		//Task<SteamGameDto?> GameDetailsAsync(int appId);
		Task<GameResponse?> GetGame(int appId);
		Task<bool> ImportGameAsync(int appId);
		//Task UpdateGamePrice(int appId);
		Task<List<Game>?> GetGameList(List<int> appIds);
	}
}
