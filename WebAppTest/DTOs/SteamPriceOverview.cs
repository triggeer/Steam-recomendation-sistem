using System.Text.Json.Serialization;

namespace WebAppTest.DTOs
{
	public class SteamPriceOverview
	{
		[JsonPropertyName("initial")]
		public int Initial { get; set; }

		[JsonPropertyName("final")]
		public int Final { get; set; }


	}
}
