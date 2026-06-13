using System.Text.Json.Serialization;

namespace WebAppTest.DTOs
{
	public class SteamGenreDto
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }
		[JsonPropertyName("description")]
		public string Description { get; set; }
	}
}
