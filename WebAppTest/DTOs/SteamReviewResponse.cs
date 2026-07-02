using System.Text.Json.Serialization;
using WebAppTest.Models;

namespace WebAppTest.DTOs
{
	public class SteamReviewResponse
	{
		[JsonPropertyName("query_summary")]
		public SteamReviewsDto data { get; set; }

	}
}
