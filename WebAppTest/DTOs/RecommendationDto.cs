using WebAppTest.Models;

namespace WebAppTest.DTOs
{
	public class RecommendationDto
	{
		public int GameId { get; set; }
		public string GameName { get; set; }
		public double CosSimilarity {  get; set; }
		public List<TagFromDb> GameTags { get; set; }
	}
}
