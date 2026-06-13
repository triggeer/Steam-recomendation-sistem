using WebAppTest.Models;

namespace WebAppTest.Interfaces
{
	public interface ICreateService
	{
		Task<Game> GameGreate(
		int appId,
		string name, 
		string detaildDescription, 
		int initialPrice, 
		List<string> genres, 
		Dictionary<string, int> tags);
		Task AddGame(Game game);
	}
}
