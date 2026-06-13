using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAppTest.DTOs
{
	public class SpyGameDto
	{
		[JsonPropertyName("appid")]
		public int AppId { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("tags")]
		public JsonElement Tags { get; set; }

		[JsonPropertyName("initialprice")]
		public string InitialPrice { get; set; }
	}
}
