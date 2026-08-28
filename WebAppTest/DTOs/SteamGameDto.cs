using System.Text.Json.Serialization;

namespace WebAppTest.DTOs
{
	public class SteamGameDto
	{
		[JsonPropertyName("steam_appid")]
		public int SteamAppId { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }
		[JsonPropertyName("genres")]
		public List<SteamGenreDto> Genres { get; set; }

		[JsonPropertyName("detailed_description")]
		public string DetailedDescription { get; set; }
		[JsonPropertyName("header_image")]
		public string ImgUrl { get; set; }
	}
}
