using WebAppTest.Models;

namespace WebAppTest.DTOs
{
	public class RecommendationDto
	{
		// Id БД
		public int GameId { get; set; }
		public string GameName { get; set; }
		public double Score {  get; set; }
		public Dictionary<int, double> GameTags { get; set; }
	}
}
