using WebAppTest.Models;

namespace WebAppTest.Interfaces
{
	public interface ICreateService
	{
		Task<Game> GameCreate(
		int appId,
		string name, 
		string imgUrl,
		string detaildDescription,
		double userScore,
		int reviewAmount,
		long owners,
		int? initialPrice,
		int? finalPrice,
		string? currency,
		double vectorLength,
		List<string> genres, 
		Dictionary<string, int> tags);
		Task AddGame(Game game);
	}
}
