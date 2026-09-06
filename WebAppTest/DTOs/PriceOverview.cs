using System.Text.Json.Serialization;

namespace WebAppTest.DTOs
{
	public class PriceOverview
	{
		[JsonPropertyName("initial")]
		public int Initial { get; set; }

		[JsonPropertyName("final")]
		public int Final { get; set; }


	}
}
