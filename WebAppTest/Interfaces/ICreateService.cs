using WebAppTest.Models;

namespace WebAppTest.Interfaces
{
	public interface ICreateService
	{
		Task<Game> GameCreate(
		int appId,
		string name, 
		string detaildDescription,
		double userScore,
		int reviewAmount,
		long owners,
		int? initialPrice,
		double vectorLength,
		List<string> genres, 
		Dictionary<string, int> tags);
		Task AddGame(Game game);
	}
}
