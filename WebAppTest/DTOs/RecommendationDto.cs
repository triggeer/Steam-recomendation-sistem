using WebAppTest.Models;

namespace WebAppTest.DTOs
{
	public class RecommendationDto
	{
		public int GameId { get; set; }
		public string GameName { get; set; }
		public double Score {  get; set; }
		public List<RecommendedGameTag> GameTags { get; set; }
	}
}
