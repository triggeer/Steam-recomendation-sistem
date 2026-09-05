using WebAppTest.DTOs;

namespace WebAppTest.Interfaces
{
	public interface IDBService
	{
		Task UpdateAllGamePrice();
		Task UpdateGamePrice(int appId);
		Task<PriceData> GetGamePrice(int steamId);
		Task UpdateAllImgAsync();
	}
}
