using System.Text.Json.Serialization;

namespace WebAppTest.DTOs
{
	public class OwnedGameDto
	{
		[JsonPropertyName("appid")]
		public int AppId { get; set; }
		[JsonPropertyName("playtime_forever")]
		public int PlayTime { get; set; }
	}
}
