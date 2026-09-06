namespace WebAppTest.DTOs
{
	public class RecommendationCandidate
	{
		public int Id { get; set; }
		public RecommendationDto Game { get; init; }
		public double UserScore { get; set; }
		public double MaxSimilarity { get; set; }
		public double FinalScore { get; set; }
	}
}
