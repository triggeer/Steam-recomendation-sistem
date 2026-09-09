using WebAppTest.DTOs;

namespace WebAppTest.Services.Recommendation.Models
{
	public class RecommendationCandidate
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public double Score { get; set; }
		public Dictionary<int, double> GameTags { get; set; }
		public double MaxSimilarity { get; set; }
		public double FinalScore { get; set; }
	}
}
