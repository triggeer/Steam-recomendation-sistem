using WebAppTest.DTOs;
using WebAppTest.Models;

namespace WebAppTest.Interfaces
{
	public interface IGameService
	{
		Task<bool> ImportGameAsync(int appId);
		Task<GameResponse?> GetGame(int appId);
		Task UpdateAllGamePrice();
		Task UpdateGamePrice(int appId);
		Task UpdateAllImgAsync();
	}
}
