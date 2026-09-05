using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAppTest.DTOs
{
	public class SpyGameDto
	{
		[JsonPropertyName("appid")]
		public int AppId { get; set; }

		[JsonPropertyName("tags")]
		public JsonElement Tags { get; set; }

		[JsonPropertyName("price")]
		[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
		public int FinalPrice { get; set; } = -1;

		[JsonPropertyName("initialprice")]
		[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
		public int InitialPrice { get; set; } = -1;
		
		[JsonPropertyName("positive")]
		[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
		public int Positive {  get; set; }

		[JsonPropertyName("negative")]
		[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
		public int Negative { get; set; }

		[JsonPropertyName("owners")]
		public string Owners { get; set ; } = "0";
	}
}
