using WebAppTest.Models;

namespace WebAppTest.DTOs
{
	public class RecommendationDto
	{
		// Id Steam
		public int GameId { get; set; }
		public string GameName { get; set; }
		public double Score {  get; set; }
		public List<RecommendedGameTag> GameTags { get; set; }
	}
}
