namespace WebAppTest.Interfaces
{
	public interface IDBService
	{
		Task UpdateAllGamePrice();
		Task UpdateGamePrice(int appId);
		Task UpdateAllImgAsync();
	}
}
