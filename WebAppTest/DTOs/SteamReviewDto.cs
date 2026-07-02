using System.Text.Json.Serialization;

namespace WebAppTest.DTOs
{
	public class SteamReviewsDto
	{
		[JsonPropertyName("total_reviews")]
		public int Total {  get; set; }

		[JsonPropertyName("total_positive")]
		public int Positive { get; set; }
	}
}
